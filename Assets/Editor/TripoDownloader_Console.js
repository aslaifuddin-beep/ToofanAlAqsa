/**
 * Tripo3D Model Downloader
 * ─────────────────────────────────────────────────────────────────────────────
 * HOW TO USE:
 * 1. Open https://studio.tripo3d.ai/3d-model/<MODEL_ID> in Chrome/Edge
 * 2. Press F12 to open DevTools
 * 3. Go to Console tab
 * 4. Paste this entire script and press Enter
 * 5. The .glb file will download automatically
 * ─────────────────────────────────────────────────────────────────────────────
 */

(async function extractTripoGLB() {
  const appEl = document.querySelector('[data-v-app]') || document.querySelector('#__nuxt');
  if (!appEl || !appEl.__vue_app__) { throw new Error('Vue app not found.'); }
  const vueApp = appEl.__vue_app__;
  const router = vueApp.config?.globalProperties?.$router;
  const route = router?.currentRoute?.value;
  const matched = route?.matched?.[0];
  if (!matched) throw new Error('No matched route found.');
  const pageInternal = matched.instances?.default?._;
  if (!pageInternal) throw new Error('Page component not found.');

  function findComponentByName(vnode, targetName, depth = 0) {
    if (!vnode || depth > 40) return null;
    if (vnode.component) {
      const inst = vnode.component;
      const name = inst.type?.name || inst.type?.__name || '';
      if (name === targetName) return inst;
      const found = findComponentByName(inst.subTree, targetName, depth + 1);
      if (found) return found;
      return null;
    }
    if (Array.isArray(vnode.children)) {
      for (const child of vnode.children) {
        if (child && typeof child === 'object') {
          const found = findComponentByName(child, targetName, depth);
          if (found) return found;
        }
      }
    }
    return null;
  }

  const contextComp = findComponentByName(pageInternal.subTree, 'Context');
  if (!contextComp) throw new Error('TresJS Context not found.');
  const tres = contextComp.provides?.useTres;
  if (!tres) throw new Error('useTres not found.');
  const scene = tres.scene?.value ?? tres.scene;
  if (!scene?.isScene) throw new Error('Scene not found.');

  function findMeshes(obj, results = []) {
    if (!obj) return results;
    if (obj.isMesh) results.push(obj);
    if (obj.children) for (const c of obj.children) findMeshes(c, results);
    return results;
  }

  const allMeshes = findMeshes(scene);
  const modelMesh = allMeshes.reduce((best, m) =>
    (m.geometry?.attributes?.position?.count ?? 0) >
    (best.geometry?.attributes?.position?.count ?? 0) ? m : best, allMeshes[0]);
  if (!modelMesh) throw new Error('No mesh found.');

  const geo = modelMesh.geometry;
  const positions = geo.attributes.position.array;
  const normals = null;
  const uvs = geo.attributes.uv?.array;
  const indices = geo.index?.array;

  let texturePNGBytes = null;
  const texImage = modelMesh.material?.map?.image;
  if (texImage) {
    const offscreen = document.createElement('canvas');
    offscreen.width = texImage.width ?? texImage.naturalWidth ?? 1024;
    offscreen.height = texImage.height ?? texImage.naturalHeight ?? 1024;
    const ctx2d = offscreen.getContext('2d');
    ctx2d.drawImage(texImage, 0, 0);
    const pngBlob = await new Promise(res => offscreen.toBlob(res, 'image/png'));
    texturePNGBytes = new Uint8Array(await pngBlob.arrayBuffer());
  }

  const align4 = n => Math.ceil(n / 4) * 4;
  const vertexCount = positions.length / 3;
  const posBytes = positions.byteLength;
  const normBytes = 0;
  const uvBytes = uvs ? align4(uvs.byteLength) : 0;
  const idxBytes = indices ? align4(indices.byteLength) : 0;
  const texBytes = texturePNGBytes ? align4(texturePNGBytes.byteLength) : 0;
  const normOffset = align4(posBytes);
  const uvOffset = normOffset + normBytes;
  const idxOffset = uvOffset + uvBytes;
  const texOffset = idxOffset + idxBytes;
  const totalBin = align4(texOffset + texBytes);

  let minPos = [Infinity, Infinity, Infinity];
  let maxPos = [-Infinity, -Infinity, -Infinity];
  for (let i = 0; i < positions.length; i += 3) {
    minPos[0] = Math.min(minPos[0], positions[i]);
    minPos[1] = Math.min(minPos[1], positions[i+1]);
    minPos[2] = Math.min(minPos[2], positions[i+2]);
    maxPos[0] = Math.max(maxPos[0], positions[i]);
    maxPos[1] = Math.max(maxPos[1], positions[i+1]);
    maxPos[2] = Math.max(maxPos[2], positions[i+2]);
  }

  const accessors = [];
  const bufferViews = [];
  const primitiveAttr = {};

  bufferViews.push({ buffer: 0, byteOffset: 0, byteLength: posBytes, target: 34962 });
  accessors.push({ bufferView: 0, byteOffset: 0, componentType: 5126, count: vertexCount, type: 'VEC3', min: minPos, max: maxPos });
  primitiveAttr.POSITION = 0;

  if (uvs) {
    const bvIdx = bufferViews.length;
    bufferViews.push({ buffer: 0, byteOffset: uvOffset, byteLength: uvs.byteLength, target: 34962 });
    accessors.push({ bufferView: bvIdx, byteOffset: 0, componentType: 5126, count: uvs.length / 2, type: 'VEC2' });
    primitiveAttr.TEXCOORD_0 = accessors.length - 1;
  }

  let indicesAccessorIdx = null;
  if (indices) {
    const bvIdx = bufferViews.length;
    bufferViews.push({ buffer: 0, byteOffset: idxOffset, byteLength: indices.byteLength, target: 34963 });
    indicesAccessorIdx = accessors.length;
    accessors.push({ bufferView: bvIdx, byteOffset: 0, componentType: 5125, count: indices.length, type: 'SCALAR' });
  }

  let imageBufferView = null;
  if (texturePNGBytes) {
    imageBufferView = bufferViews.length;
    bufferViews.push({ buffer: 0, byteOffset: texOffset, byteLength: texturePNGBytes.byteLength });
  }

  const primitive = { attributes: primitiveAttr };
  if (indicesAccessorIdx !== null) primitive.indices = indicesAccessorIdx;
  if (imageBufferView !== null) primitive.material = 0;

  const gltf = {
    asset: { version: '2.0', generator: 'tripo3d-browser-extractor' },
    scene: 0, scenes: [{ name: 'Scene', nodes: [0] }],
    nodes: [{ name: modelMesh.name || 'tripo_model', mesh: 0 }],
    meshes: [{ name: 'tripo_mesh', primitives: [primitive] }],
    accessors, bufferViews,
    buffers: [{ byteLength: totalBin }],
    ...(imageBufferView !== null ? {
      materials: [{ name: 'tripo_mat', pbrMetallicRoughness: { baseColorTexture: { index: 0 }, metallicFactor: 0.0, roughnessFactor: 0.5 }, doubleSided: true }],
      textures: [{ source: 0 }],
      images: [{ mimeType: 'image/png', bufferView: imageBufferView }]
    } : {})
  };

  const jsonEncoded = new TextEncoder().encode(JSON.stringify(gltf));
  const jsonPadded = align4(jsonEncoded.length);
  const totalSize = 12 + 8 + jsonPadded + 8 + totalBin;
  const glb = new ArrayBuffer(totalSize);
  const dv = new DataView(glb);
  const buf = new Uint8Array(glb);
  let off = 0;

  dv.setUint32(off, 0x46546C67, true); off += 4;
  dv.setUint32(off, 2, true); off += 4;
  dv.setUint32(off, totalSize, true); off += 4;
  dv.setUint32(off, jsonPadded, true); off += 4;
  dv.setUint32(off, 0x4E4F534A, true); off += 4;
  buf.set(jsonEncoded, off);
  for (let i = jsonEncoded.length; i < jsonPadded; i++) buf[off + i] = 0x20;
  off += jsonPadded;
  dv.setUint32(off, totalBin, true); off += 4;
  dv.setUint32(off, 0x004E4942, true); off += 4;

  buf.set(new Uint8Array(positions.buffer, positions.byteOffset, positions.byteLength), off);
  if (uvs) buf.set(new Uint8Array(uvs.buffer, uvs.byteOffset, uvs.byteLength), off + uvOffset);
  if (indices) buf.set(new Uint8Array(indices.buffer, indices.byteOffset, indices.byteLength), off + idxOffset);
  if (texturePNGBytes) buf.set(texturePNGBytes, off + texOffset);

  const modelId = window.location.pathname.split('/').filter(Boolean).pop() || 'model';
  const filename = `tripo_${modelId}.glb`;
  const blob = new Blob([glb], { type: 'model/gltf-binary' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url; a.download = filename; a.click();
  setTimeout(() => URL.revokeObjectURL(url), 10000);
  console.log(`Downloaded: ${filename} (${(totalSize/1024/1024).toFixed(2)} MB)`);
})();
