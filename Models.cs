using System;

namespace AGDRONE
{
    public interface ICommunicator
    {
        void TransmitData();
    }

    public class GPSModule
    {
        public string Coordinates { get; set; }
        public double X { get; private set; }
        public double Y { get; private set; }

        public GPSModule(string coordinates)
        {
            Coordinates = coordinates;
            ParseCoordinates(coordinates);
        }

        public void UpdateLocation(double x, double y)
        {
            X = Math.Round(x, 2);
            Y = Math.Round(y, 2);
            Coordinates = $"{X}, {Y}";
        }

        private void ParseCoordinates(string coords)
        {
            try
            {
                var parts = coords.Split(',');
                if (parts.Length >= 2)
                {
                    X = double.Parse(parts[0].Trim());
                    Y = double.Parse(parts[1].Trim());
                    return;
                }
            }
            catch { }
            X = 0; Y = 0;
        }

        public override string ToString() => Coordinates;
    }

    public abstract class Drone : ICommunicator
    {
        // STATIC MEMBER DEMO 
        public static int TotalDronesDeployed { get; set; } = 0;
        
        // GLOBAL LOG DEMO
        public static List<string> GlobalLog { get; } = new List<string>();

        public string ID { get; set; }
        public int BatteryLevel { get; set; }
        public string Type { get; set; }
        
        [System.ComponentModel.Browsable(false)]
        public GPSModule GPS { get; set; }
        
        public string GPS_Location => GPS.Coordinates;
        
        // This is a generic property used for DataGridView display (represents Payload, Resolution, etc.)
        public string SpecialAttribute { get; set; }

        protected Drone(string id, int batteryLevel, string coordinates, string type)
        {
            ID = id;
            BatteryLevel = batteryLevel;
            GPS = new GPSModule(coordinates);
            Type = type;
            
            // Increment static counter
            TotalDronesDeployed++;
        }

        public void TransmitData()
        {
            Console.WriteLine($"Drone {ID} transmitting telemetry...");
        }

        public abstract string PerformMission();

        public virtual string DisplayStatus()
        {
            return $"[{Type}] ID: {ID} | Battery: {BatteryLevel}% | Loc: {GPS.Coordinates}";
        }

        public void EmergencyRecall()
        {
            BatteryLevel = 100;
            GPS.Coordinates = "Base Station";
        }

        // Operator Overloading Demo
        public static bool operator >(Drone d1, Drone d2)
        {
            if (d1 is null || d2 is null) return false;
            return d1.BatteryLevel > d2.BatteryLevel;
        }

        public static bool operator <(Drone d1, Drone d2)
        {
            if (d1 is null || d2 is null) return false;
            return d1.BatteryLevel < d2.BatteryLevel;
        }
    }

    public class DeliveryDrone : Drone
    {
        public double PayloadWeight { get; set; }

        public DeliveryDrone(string id, int battery, string coords, double payload) 
            : base(id, battery, coords, "Delivery")
        {
            PayloadWeight = payload;
            SpecialAttribute = $"{payload} kg";
        }

        public override string PerformMission() => $"Transporting {PayloadWeight}kg package.";
    }

    public class SurveillanceDrone : Drone
    {
        public string CameraResolution { get; set; }

        public SurveillanceDrone(string id, int battery, string coords, string resolution) 
            : base(id, battery, coords, "Surveillance")
        {
            CameraResolution = resolution;
            SpecialAttribute = resolution;
        }

        public override string PerformMission() => $"Streaming video at {CameraResolution}.";
    }

    public class AgriculturalDrone : Drone
    {
        public string FertilizerType { get; set; }

        public AgriculturalDrone(string id, int battery, string coords, string fertilizer) 
            : base(id, battery, coords, "Agricultural")
        {
            FertilizerType = fertilizer;
            SpecialAttribute = fertilizer;
        }

        public override string PerformMission() => $"Dispersing {FertilizerType}.";
    }

    public class MedicalDrone : Drone
    {
        public string MedicalSupplyType { get; set; }

        public MedicalDrone(string id, int battery, string coords, string supply) 
            : base(id, battery, coords, "Medical")
        {
            MedicalSupplyType = supply;
            SpecialAttribute = supply;
        }

        public override string PerformMission() => $"Delivering {MedicalSupplyType}.";
    }

    public class RacingDrone : Drone
    {
        public int TopSpeed { get; set; }

        public RacingDrone(string id, int battery, string coords, int speed) 
            : base(id, battery, coords, "Racing")
        {
            TopSpeed = speed;
            SpecialAttribute = $"{speed} km/h";
        }

        public override string PerformMission() => $"Racing at {TopSpeed} km/h.";
    }

    public enum FormationPattern
    {
        V_Shape,
        Echelon,
        Column,
        LineAbreast
    }

    public class DroneSwarm : ICommunicator
    {
        public string SwarmName { get; set; }
        private System.Collections.Generic.List<Drone> activeDrones = new System.Collections.Generic.List<Drone>();

        public Drone Leader => activeDrones.Count > 0 ? activeDrones[0] : null;
        public System.Collections.Generic.IReadOnlyList<Drone> Drones => activeDrones.AsReadOnly();

        public DroneSwarm(string name)
        {
            SwarmName = name;
        }

        public void AddDrone(Drone drone)
        {
            if (!activeDrones.Contains(drone))
            {
                activeDrones.Add(drone);
                Drone.GlobalLog.Add($"[Swarm: {SwarmName}] Added Drone {drone.ID}");
            }
        }

        public void RemoveDrone(Drone drone)
        {
            if (activeDrones.Remove(drone))
            {
                Drone.GlobalLog.Add($"[Swarm: {SwarmName}] Removed Drone {drone.ID}");
            }
        }

        public void TransmitData()
        {
            Console.WriteLine($"Swarm {SwarmName} transmitting collective telemetry...");
            foreach (var drone in activeDrones)
            {
                drone.TransmitData();
            }
        }

        // Formation Flying (Calculates offsets relative to the leader)
        public void FormUp(FormationPattern pattern)
        {
            if (activeDrones.Count < 2) return;

            Drone.GlobalLog.Add($"[Swarm: {SwarmName}] Initiating {pattern} formation.");

            var leader = Leader;
            double baseX = leader.GPS.X;
            double baseY = leader.GPS.Y;

            for (int i = 1; i < activeDrones.Count; i++)
            {
                var drone = activeDrones[i];
                double offsetX = 0, offsetY = 0;

                switch (pattern)
                {
                    case FormationPattern.V_Shape:
                        offsetX = (i % 2 != 0 ? -1 : 1) * 10 * ((i + 1) / 2);
                        offsetY = -10 * ((i + 1) / 2);
                        break;
                    case FormationPattern.LineAbreast:
                        offsetX = (i % 2 != 0 ? -1 : 1) * 15 * ((i + 1) / 2);
                        offsetY = 0;
                        break;
                    case FormationPattern.Column:
                        offsetX = 0;
                        offsetY = -15 * i;
                        break;
                }

                drone.GPS.UpdateLocation(baseX + offsetX, baseY + offsetY);
                Drone.GlobalLog.Add($"[Swarm] {drone.ID} moved to {drone.GPS.Coordinates}");
            }
        }

        // Collision Avoidance Algorithm
        public void CheckCollisionAvoidance()
        {
            double safeDistance = 5.0; // Minimum safe distance in coordinate units
            for (int i = 0; i < activeDrones.Count; i++)
            {
                for (int j = i + 1; j < activeDrones.Count; j++)
                {
                    var d1 = activeDrones[i];
                    var d2 = activeDrones[j];

                    // Euclidean distance
                    double distance = Math.Sqrt(Math.Pow(d1.GPS.X - d2.GPS.X, 2) + Math.Pow(d1.GPS.Y - d2.GPS.Y, 2));

                    if (distance < safeDistance)
                    {
                        Drone.GlobalLog.Add($"[WARNING] Collision risk between {d1.ID} & {d2.ID}. Initiating evasion!");

                        // Autonomous Correction: Move d2 to safety
                        d2.GPS.UpdateLocation(d2.GPS.X + safeDistance * 1.5, d2.GPS.Y + safeDistance * 1.5);
                    }
                }
            }
        }

        // Task Distribution based on Polymorphic Drone Types
        public void ExecuteSwarmMission(string missionBrief)
        {
            Drone.GlobalLog.Add($"[Swarm: {SwarmName}] Commencing assigned mission: {missionBrief}");

            foreach (var drone in activeDrones)
            {
                if (drone is SurveillanceDrone sd)
                    Drone.GlobalLog.Add($"[Task] {sd.ID} providing aerial reconnaissance -> {sd.PerformMission()}");
                else if (drone is DeliveryDrone dd)
                    Drone.GlobalLog.Add($"[Task] {dd.ID} handling heavy lifting -> {dd.PerformMission()}");
                else if (drone is MedicalDrone md)
                    Drone.GlobalLog.Add($"[Task] {md.ID} deploying critical supplies -> {md.PerformMission()}");
                else
                    Drone.GlobalLog.Add($"[Task] {drone.ID} supporting -> {drone.PerformMission()}");
            }
        }
    }
}


