# طوفان الأقصى - FPS Game (Unity Android)

## 📋 Overview

"طوفان الأقصى" is a first-person shooter (FPS) game built with Unity, set in Gaza. 
The player controls a Qassam Brigades fighter through destroyed streets, underground tunnels, and enemy territory.

**Engine:** Unity 2022.3.20f1 (URP)
**Platform:** Android (APK)
**Genre:** Tactical FPS / AAA Cinematic

---

## 🚀 Quick Start

### Prerequisites
1. Unity Hub + Unity 2022.3.20f1 installed
2. Android Build Support module (IL2CPP, Vulkan)
3. JDK 11+, Android SDK, Android NDK

### Setup Steps

1. **Open the project** in Unity Hub
2. **Install URP** if not already installed:
   - Window > Package Manager > Universal RP (install)
   - Create URP Asset (Assets > Create > Rendering > URP Asset)
   - Assign in Graphics Settings

3. **Create a scene** named `MainScene` in `Assets/Scenes/`
4. **Add to Build Settings**: File > Build Settings > Drag MainScene

5. **Configure Player Settings** (File > Build Settings > Player Settings):
   - Package Name: `com.qassam.toofanalaqsa`
   - Minimum API Level: 26 (Android 8.0)
   - Target API Level: 33
   - Scripting Backend: IL2CPP
   - Target Architecture: ARM64
   - Graphics API: Vulkan
   - Texture Compression: ASTC
   - Multithreaded Rendering: ✓

6. **Assign Scripts to GameObjects** (see Scene Setup below)

---

## 🎬 Scene Setup Instructions

### Essential GameObjects

Create these in your scene and attach the scripts:

```
/GameManager          → GameManager.cs
/MainCamera           → Camera, FPSController.cs (cameraHolder), CameraEffects.cs
/PlayerController     → CharacterController, FPSController.cs
/WeaponHolder         → (child of PlayerController)
/UIManager            → UIManager.cs
/AudioManager         → AudioManager.cs
/EventSystem          → (from UI menu)
/TouchCanvas          → Canvas (Screen Space - Overlay), TouchControls.cs
/UpgradeSystem        → UpgradeSystem.cs
```

### Enemy Setup
```
/Enemies/Soldier_01   → NavMeshAgent, TacticalEnemyAI.cs, HealthSystem.cs
/Enemies/Tank_01      → NavMeshAgent, TankAI.cs, HealthSystem.cs
```

### Mission Setup
```
/MissionManager       → MissionManager.cs
/MissionZones/Zone_01 → Trigger collider, AudioReverbZone.cs
```

### NavMesh
- Window > AI > Navigation
- Bake NavMesh on all walkable surfaces
- Enemies use NavMeshAgent for pathfinding

---

## 🎮 Controls

### Keyboard (Editor/PC)
| Key | Action |
|-----|--------|
| WASD | Move |
| Mouse | Look/Aim |
| Left Click | Fire |
| Right Click | ADS (Aim Down Sight) |
| R | Reload |
| 1,2,3 | Switch Weapons (AR/Sniper/RPG) |
| Shift | Sprint |
| C | Crouch |
| Space | Jump |
| U | Upgrade Menu |
| ESC | Pause |

### Touch (Mobile)
- Left joystick: Movement
- Right joystick: Look
- On-screen buttons: Fire, Reload, Switch, Crouch, Sprint, Jump, Zoom
- Customization: long-press during gameplay to rearrange buttons

---

## 🗺️ Mission System

The game includes 3 built-in missions:

1. **طوفان الأقصى - مقدمة** (Tutorial)
   - Destroy 3 military vehicles
   - Secure residential area (5 enemies)
   - Reach tunnel entrance

2. **الاشتباك في الأنفاق** (Tunnel Combat)
   - Eliminate 10 soldiers in tunnels
   - Defend command post (60 seconds)

3. **معركة مفتوحة** (Open Battle)
   - Destroy 5 Merkava tanks with RPG
   - Eliminate 15 soldiers in streets
   - Reach victory square

---

## 📁 Project Structure

```
Assets/
├── Scripts/           # All C# source code
│   ├── FPSController.cs
│   ├── WeaponManager.cs
│   ├── YasinRPG_Projectile.cs
│   ├── TacticalEnemyAI.cs
│   ├── TankAI.cs
│   ├── MissionManager.cs
│   ├── AudioManager.cs
│   ├── AudioReverbZone.cs
│   ├── UpgradeSystem.cs
│   ├── HealthSystem.cs
│   ├── DamageSystem.cs
│   ├── BulletProjectile.cs
│   ├── CameraEffects.cs
│   ├── UIManager.cs
│   ├── TouchControls.cs
│   ├── ButtonCustomizer.cs
│   └── GameManager.cs
├── Editor/
│   └── BuildHelper.cs
├── Resources/         # Runtime-loaded assets
├── Scenes/            # Unity scenes
├── Audio/             # Sound files (.wav/.mp3)
├── Prefabs/           # Reusable prefabs
└── UI/                # UI components
```

---

## 🔫 Weapons System

| Weapon | Type | Ammo | Damage | Special |
|--------|------|------|--------|---------|
| Assault Rifle | Automatic | 30 | 35 | All-purpose |
| Sniper Rifle | Semi-auto | 5 | 80 | Headshot 2.5x |
| RPG (ياسين 100) | Projectile | 1 | 200 | Heavy explosion |
| RPG (ياسين 5) | Projectile | 1 | 150 | Faster, guided |

### RPG Switching
Press the weapon switch button to toggle between Yasin 100 and Yasin 5 rockets.
- **ياسين 100**: High damage, large blast radius
- **ياسين 5**: Faster velocity, slight homing capability

---

## 🧠 Enemy AI States

- **Patrol**: Wanders within a set radius
- **Alert**: Investigates last known player position
- **Combat**: Engages player with accurate fire
- **Flanking**: Circles around player position
- **Cover**: Takes cover behind obstacles
- **Suppressed**: Retreats under heavy fire
- **Dead**: Ragdoll/destroy

---

## 📱 Building the APK

### Method 1: Unity Editor
1. Tools > Build Android APK (menu item)
2. Or: File > Build Settings > Build

### Method 2: Command Line
```bash
/path/to/Unity -quit -batchmode -buildTarget Android \
  -projectPath /path/to/project \
  -executeMethod BuildHelper.BuildAPK
```

### Method 3: Manual
1. File > Build Settings
2. Select Android platform, Switch Platform
3. Check Development Build for testing
4. Click Build
5. Transfer APK to phone and install

---

## 🎨 Adding 3D Models & Audio

Read `Assets/Resources/PLACE_ASSETS_HERE.txt` for detailed instructions.

### Where to find free assets:
- **Characters:** mixamo.com (free animations too)
- **Weapons:** Unity Asset Store (search "FPS weapons")
- **Environment:** Sketchfab, TurboSquid, CGTrader
- **Audio:** freesound.org, zapsplat.com

### Quick test without assets:
All scripts use placeholder primitives (cubes, spheres) when real models
are missing. The game is fully playable with just the code.

---

## ⚙️ Performance Optimization

### For smooth 60 FPS on mobile:
1. Use URP with 1-2 directional lights max
2. Enable GPU instancing on materials
3. Use LOD groups on complex models
4. Limit draw calls (< 200 per frame)
5. Use occlusion culling
6. Compress textures to ASTC 6x6
7. Set quality level to "Medium" or "High"

### Script settings for mobile:
- Reduce `maxDistance` in BulletProjectile.cs (line: 17)
- Reduce `detectionRange` in TacticalEnemyAI.cs (line: 15)
- Lower `particleRaycastBudget` in QualitySettings

---

## 📝 Notes

- The game uses `Resources.Load()` for asset management
- All scripts include comments in English with Arabic mission text
- Mission text appears in Arabic (UI supports RTL with proper font)
- Enemy AI uses Unity NavMesh system (bake NavMesh for your level)

---

**Developed with OpenCode | كتابة الأكواد بواسطة OpenCode**
**للإصدار 1.0 - طوفان الأقصى**
