using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PerformanceController
{
    public struct MapPerfomanceReport
    {
        // Last Map
        // Landscape
        public float landPhase;
        public float mountainPhase;
        public float forestPhase;

        // Settlements
        public float playerBase;
        public float roadTracing;

        // Resources
        public float neutral;
        public float symmetric;
    }

    private static List<MapPerfomanceReport> mapPerfomanceReports = new List<MapPerfomanceReport>();

    public static void addMapPerformaceReport(MapPerfomanceReport report)
    {
        mapPerfomanceReports.Add(report);
    }

    public static MapPerfomanceReport getLastMapPerformanceReport()
    {
        return mapPerfomanceReports[mapPerfomanceReports.Count - 1];
    }

    public static MapPerfomanceReport getMeanMapPerformanceReport()
    {
        MapPerfomanceReport globalReport;
        float meanLandPhase = 0;
        float meanMountainPhase = 0;
        float meanForestPhase = 0;

        float meanPlayerBase = 0;
        float meanRoadTracing = 0;

        float meanNeutral = 0;
        float meanSymmetric = 0;

        foreach(MapPerfomanceReport mpr in mapPerfomanceReports)
        {
            meanLandPhase += mpr.landPhase;
            meanMountainPhase += mpr.mountainPhase;
            meanForestPhase += mpr.forestPhase;

            meanPlayerBase += mpr.playerBase;
            meanRoadTracing += mpr.roadTracing;

            meanNeutral += mpr.neutral;
            meanSymmetric += mpr.symmetric;
        }

        int total = mapPerfomanceReports.Count;
        globalReport.landPhase = meanLandPhase / total;
        globalReport.mountainPhase = meanMountainPhase / total;
        globalReport.forestPhase = meanForestPhase / total;

        globalReport.playerBase = meanPlayerBase / total;
        globalReport.roadTracing = meanRoadTracing / total;

        globalReport.neutral = meanNeutral / total;
        globalReport.symmetric = meanSymmetric / total;

        return globalReport;
    }

    public static int getReportAmount()
    {
        return mapPerfomanceReports.Count;
    }
}
