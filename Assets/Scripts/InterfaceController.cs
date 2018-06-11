using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceController : MonoBehaviour {

    public WorldGenerator worldGenerator;
    public Camera mapCamera;

    // InputFields

        // Generation Options
    public InputField seedInputField;
    public InputField widthInputField;
    public InputField depthInputField;

        // Seed Count
    public InputField landSeedCountInputField;
    public InputField mountainSeedCountInputField;
    public InputField forestSeedCountInputField;

        // Resource Count
    public InputField alliedResourceCountInputField;
    public InputField neutralResourceCountInputField;
    public InputField symmetricResourceCountInputField;
    public Toggle oldMehtodCheckBox;

        // Misc Options
    public Toggle roadsToggle;
    public Toggle resourcesToggle;
    public Toggle autoAdjustToggle;
    public Toggle expositionMode;

    public Button generateButton;

    // Performance Panel
    public GameObject performancePanel;

    // Last Map

    // LandScape
    public Text landPhaseTime;
    public Text mountainPhaseTime;
    public Text forestPhaseTime;

    // Settlements
    public Text playerBasesTime;
    public Text roadTracingTime;

    // Resources
    public Text neutralResourcesTime;
    public Text symmetricResourceTime;

    public Text lastTotal;

    // Global
    public Text genAmount;

    public Text globalLandPhaseTime;
    public Text globalMountainPhaseTime;
    public Text globalForestTime;

    public Text globalPlayerBasesTime;
    public Text globalRoadTracingTime;

    public Text globalNeutralResourcesTime;
    public Text globalSymmetricResourceTime;

    public Text avgTotal;

    private void Update()
    {
        int seed = int.Parse(seedInputField.text);
        int width = int.Parse(widthInputField.text);
        int depth = int.Parse(depthInputField.text);
        

        int landSeedCount = int.Parse(landSeedCountInputField.text);
        int mountainSeedCount = int.Parse(mountainSeedCountInputField.text);
        int forestSeedCount = int.Parse(forestSeedCountInputField.text);

        int alliedResourceCount = int.Parse(alliedResourceCountInputField.text);
        int neutralResourceCount = int.Parse(neutralResourceCountInputField.text);
        int symmetricResourceCount = int.Parse(symmetricResourceCountInputField.text);
        bool oldMethod = oldMehtodCheckBox.isOn;

        bool enableRoads = roadsToggle.isOn;
        bool enableResources = resourcesToggle.isOn;
        bool autoAdjust = autoAdjustToggle.isOn;

        worldGenerator.seedValue = seed;
        worldGenerator.length = width;
        worldGenerator.height = depth;

        worldGenerator.seedCount = landSeedCount;
        worldGenerator.forestSeedCount = forestSeedCount;
        worldGenerator.mountainSeedCount = mountainSeedCount;

        worldGenerator.alliedResourceCount = alliedResourceCount;
        worldGenerator.neutralResourceCount = neutralResourceCount;
        worldGenerator.asymmetricResourceCount = symmetricResourceCount;
        worldGenerator.oldPlayerBasePlacement = oldMethod;

        worldGenerator.roads = enableRoads;
        worldGenerator.resources = enableResources;
        worldGenerator.autoAdjust = autoAdjust;
    }

    public void fillWithRandomValues()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        int width = 18;
        int depth = 10;

        int landSeedCount = Random.Range(0, 3);
    }

    public void generateButtonHandler()
    {
        worldGenerator.generateNewMap();
    }

    public void OnToggleExpositionMode()
    {
       if(expositionMode.isOn)
       {
            generateButton.interactable = false;
            worldGenerator.expositionMode = true;
       }
       else
        {
            generateButton.interactable = true;
            worldGenerator.expositionMode = false;
        }
    }

    public void handleInfoButton()
    {
        updateMapPerformanceReport();
        performancePanel.SetActive(true);
        mapCamera.gameObject.SetActive(false);
    }

    public void handlePerformancePanelOkButton()
    {
        performancePanel.SetActive(false);
        mapCamera.gameObject.SetActive(true);
    }

    public void updateMapPerformanceReport()
    {
        PerformanceController.MapPerfomanceReport lastMapReport = PerformanceController.getLastMapPerformanceReport();

        landPhaseTime.text = lastMapReport.landPhase.ToString();
        mountainPhaseTime.text = lastMapReport.mountainPhase.ToString();
        forestPhaseTime.text = lastMapReport.forestPhase.ToString();

        playerBasesTime.text = lastMapReport.playerBase.ToString();
        roadTracingTime.text = lastMapReport.roadTracing.ToString();

        neutralResourcesTime.text = lastMapReport.neutral.ToString();
        symmetricResourceTime.text = lastMapReport.symmetric.ToString();

        lastTotal.text = (lastMapReport.landPhase + lastMapReport.mountainPhase + lastMapReport.forestPhase
            + lastMapReport.playerBase + lastMapReport.roadTracing + lastMapReport.neutral + lastMapReport.symmetric).ToString();

        PerformanceController.MapPerfomanceReport avgMapReport = PerformanceController.getMeanMapPerformanceReport();

        genAmount.text = PerformanceController.getReportAmount().ToString();

        globalLandPhaseTime.text = avgMapReport.landPhase.ToString();
        globalMountainPhaseTime.text = avgMapReport.mountainPhase.ToString();
        globalForestTime.text = avgMapReport.forestPhase.ToString();

        globalPlayerBasesTime.text = avgMapReport.playerBase.ToString();
        globalRoadTracingTime.text = avgMapReport.roadTracing.ToString();

        globalNeutralResourcesTime.text = avgMapReport.neutral.ToString();
        globalSymmetricResourceTime.text = avgMapReport.symmetric.ToString();

        avgTotal.text = (avgMapReport.landPhase + avgMapReport.mountainPhase + avgMapReport.forestPhase
            + avgMapReport.playerBase + avgMapReport.roadTracing + avgMapReport.neutral + avgMapReport.symmetric).ToString();
    }
}
