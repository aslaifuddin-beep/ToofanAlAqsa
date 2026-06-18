using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MissionManager : MonoBehaviour
{
    [System.Serializable]
    public class MissionStage
    {
        public string stageName;
        public string description;
        public MissionType type;
        public int requiredCount;
        public float radius;
        public Vector3 targetPosition;
        public Transform targetTransform;
        public GameObject objectiveMarkerPrefab;

        [HideInInspector] public int currentProgress;
        [HideInInspector] public bool isComplete;
    }

    public enum MissionType
    {
        DestroyVehicles,
        EliminateEnemies,
        LiberateZone,
        ProtectTarget,
        ReachLocation
    }

    [System.Serializable]
    public class Mission
    {
        public string missionName;
        public string missionBriefing;
        public List<MissionStage> stages = new List<MissionStage>();
        public int xpReward = 500;

        [HideInInspector] public int currentStage;
        [HideInInspector] public bool isActive;
        [HideInInspector] public bool isComplete;
    }

    [Header("Missions")]
    public List<Mission> missions = new List<Mission>();
    public int currentMissionIndex;

    [Header("UI")]
    public Text missionNameText;
    public Text objectiveText;
    public Text progressText;
    public GameObject missionCompletePanel;
    public Text missionCompleteRewardText;

    [Header("Timers")]
    public float protectTargetDuration = 60f;
    public float missionStartDelay = 2f;

    private Mission currentMission;
    private MissionStage currentStage;
    private float protectTimer;
    private List<GameObject> activeMarkers = new List<GameObject>();

    public static MissionManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        DefineDefaultMissions();
    }

    void Update()
    {
        if (currentMission == null || !currentMission.isActive) return;

        currentStage = currentMission.stages[currentMission.currentStage];

        if (currentStage.isComplete)
            AdvanceStage();
        else
            UpdateCurrentStage();

        UpdateUI();
    }

    void DefineDefaultMissions()
    {
        Mission m1 = new Mission
        {
            missionName = "طوفان الأقصى - مقدمة",
            missionBriefing = "تدمير آليات العدو في المدينة المحتلة. استخدم كافة الأسلحة المتاحة.",
            xpReward = 500,
            stages = new List<MissionStage>
            {
                new MissionStage
                {
                    stageName = "تدمير الآليات",
                    description = "دمّر 3 آليات عسكرية للعدو",
                    type = MissionType.DestroyVehicles,
                    requiredCount = 3
                },
                new MissionStage
                {
                    stageName = "تحرير المنطقة",
                    description = "تأمين المربع السكني من قوات الاحتلال",
                    type = MissionType.LiberateZone,
                    requiredCount = 5
                },
                new MissionStage
                {
                    stageName = "الوصول إلى النفق",
                    description = "الوصول إلى مدخل النفق الأرضي",
                    type = MissionType.ReachLocation,
                    requiredCount = 1
                }
            }
        };

        missions.Add(m1);

        Mission m2 = new Mission
        {
            missionName = "الاشتباك في الأنفاق",
            missionBriefing = "طارد قوات العدو داخل شبكة الأنفاق تحت الأرض.",
            xpReward = 1000,
            stages = new List<MissionStage>
            {
                new MissionStage
                {
                    stageName = "تطهير النفق",
                    description = "تصفية 10 جنود في النفق",
                    type = MissionType.EliminateEnemies,
                    requiredCount = 10
                },
                new MissionStage
                {
                    stageName = "حماية القائد",
                    description = "احمِ موقع القيادة لمدة 60 ثانية",
                    type = MissionType.ProtectTarget,
                    requiredCount = 1
                }
            }
        };

        missions.Add(m2);

        Mission m3 = new Mission
        {
            missionName = "معركة مفتوحة",
            missionBriefing = "اشتباك شامل مع قوات العدو. استخدم الدبابات والصواريخ.",
            xpReward = 1500,
            stages = new List<MissionStage>
            {
                new MissionStage
                {
                    stageName = "تدمير الدبابات",
                    description = "دمّر 5 دبابات ميركافا باستخدام الياسين",
                    type = MissionType.DestroyVehicles,
                    requiredCount = 5
                },
                new MissionStage
                {
                    stageName = "تطهير الشوارع",
                    description = "تصفية 15 جندياً في الشوارع",
                    type = MissionType.EliminateEnemies,
                    requiredCount = 15
                },
                new MissionStage
                {
                    stageName = "رفع العلم",
                    description = "الوصول إلى ساحة العلم ورفع راية النصر",
                    type = MissionType.ReachLocation,
                    requiredCount = 1
                }
            }
        };

        missions.Add(m3);
    }

    public void StartMission(int index)
    {
        if (index < 0 || index >= missions.Count) return;

        currentMissionIndex = index;
        currentMission = missions[index];
        currentMission.currentStage = 0;
        currentMission.isActive = true;
        currentMission.isComplete = false;

        foreach (var stage in currentMission.stages)
        {
            stage.currentProgress = 0;
            stage.isComplete = false;
        }

        currentStage = currentMission.stages[0];
        ClearMarkers();
        SpawnStageMarker(currentStage);
    }

    void AdvanceStage()
    {
        currentMission.currentStage++;

        if (currentMission.currentStage >= currentMission.stages.Count)
        {
            CompleteMission();
            return;
        }

        currentStage = currentMission.stages[currentMission.currentStage];
        ClearMarkers();
        SpawnStageMarker(currentStage);
    }

    void UpdateCurrentStage()
    {
        if (currentStage == null) return;

        switch (currentStage.type)
        {
            case MissionType.EliminateEnemies:
            case MissionType.LiberateZone:
                break;

            case MissionType.ProtectTarget:
                protectTimer += Time.deltaTime;
                currentStage.currentProgress = Mathf.FloorToInt(protectTimer);
                if (protectTimer >= protectTargetDuration)
                    currentStage.isComplete = true;
                break;

            case MissionType.ReachLocation:
                if (currentStage.targetTransform != null)
                {
                    float dist = Vector3.Distance(transform.position, currentStage.targetTransform.position);
                    if (dist < currentStage.radius)
                        currentStage.isComplete = true;
                }
                break;
        }
    }

    public void RegisterProgress(MissionType type, int amount = 1)
    {
        if (currentMission == null || !currentMission.isActive) return;
        if (currentStage == null || currentStage.isComplete) return;

        if (currentStage.type == type)
        {
            currentStage.currentProgress += amount;
            if (currentStage.currentProgress >= currentStage.requiredCount)
                currentStage.isComplete = true;
        }
    }

    void CompleteMission()
    {
        currentMission.isActive = false;
        currentMission.isComplete = true;

        GameManager.Instance?.CompleteMission();

        if (missionCompletePanel != null)
        {
            missionCompletePanel.SetActive(true);
            if (missionCompleteRewardText != null)
            {
                missionCompleteRewardText.text = $"+{currentMission.xpReward} XP";
            }
        }

        ClearMarkers();
    }

    void SpawnStageMarker(MissionStage stage)
    {
        if (stage.objectiveMarkerPrefab == null) return;

        Vector3 pos = stage.targetTransform != null ?
            stage.targetTransform.position : stage.targetPosition;

        GameObject marker = Instantiate(stage.objectiveMarkerPrefab, pos, Quaternion.identity);
        activeMarkers.Add(marker);
    }

    void ClearMarkers()
    {
        foreach (var marker in activeMarkers)
            Destroy(marker);
        activeMarkers.Clear();
    }

    void UpdateUI()
    {
        if (missionNameText != null && currentMission != null)
            missionNameText.text = currentMission.missionName;

        if (objectiveText != null && currentStage != null)
            objectiveText.text = currentStage.description;

        if (progressText != null && currentStage != null)
        {
            if (currentStage.type == MissionType.ProtectTarget)
                progressText.text = $"{Mathf.FloorToInt(protectTimer)} / {protectTargetDuration}";
            else
                progressText.text = $"{currentStage.currentProgress} / {currentStage.requiredCount}";
        }
    }

    void OnDrawGizmosSelected()
    {
        if (currentStage == null) return;

        Gizmos.color = Color.cyan;
        Vector3 pos = currentStage.targetTransform != null ?
            currentStage.targetTransform.position : currentStage.targetPosition;
        Gizmos.DrawWireSphere(pos, currentStage.radius);
    }
}
