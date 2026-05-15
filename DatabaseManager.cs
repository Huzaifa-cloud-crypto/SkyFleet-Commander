using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AGDRONE
{
    public static class DatabaseManager
    {
        private const string DbFileName = "dronefleet.accdb";
        private static string ConnectionString
        {
            get
            {
                // To prevent the "Visual Studio bin\Debug vs Project Directory" confusion, 
                // we'll make the app point to the database in your main project folder during development.
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string dbPath = DbFileName; // Default to local folder for production
                
                int binIndex = baseDir.IndexOf(@"\bin\", StringComparison.OrdinalIgnoreCase);
                if (binIndex >= 0)
                {
                    // If running from bin\Debug\..., point to the root project folder's database
                    dbPath = System.IO.Path.Combine(baseDir.Substring(0, binIndex), DbFileName);
                }

                return $@"Provider=Microsoft.ACE.OLEDB.16.0;Data Source={dbPath};";
            }
        }

        public static void InitializeDatabase()
        {
            if (!File.Exists(DbFileName))
            {
                // Create the Access database file using ADOX via COM interop
                CreateAccessDatabase();

                // Create table and seed data
                using (var connection = new OleDbConnection(ConnectionString))
                {
                    connection.Open();

                    // Create the DroneTypes lookup table (for INNER JOIN queries)
                    var createTypesTable = connection.CreateCommand();
                    createTypesTable.CommandText = @"
                        CREATE TABLE tblDroneTypes (
                            TypeName TEXT(50) PRIMARY KEY,
                            Category TEXT(50) NOT NULL,
                            Description TEXT(200) NOT NULL
                        )";
                    createTypesTable.ExecuteNonQuery();

                    // Create the Drones table with foreign key to tblDroneTypes
                    var createDronesTable = connection.CreateCommand();
                    createDronesTable.CommandText = @"
                        CREATE TABLE tblDrones (
                            DroneID TEXT(50) PRIMARY KEY,
                            DroneType TEXT(50) NOT NULL,
                            BatteryLevel INTEGER NOT NULL,
                            Coordinates TEXT(50) NOT NULL,
                            SpecialAttribute TEXT(100) NOT NULL
                        )";
                    createDronesTable.ExecuteNonQuery();

                    // Seed lookup data and drone data
                    SeedDroneTypes(connection);
                    SeedData(connection);
                }
            }
        }
        private static void CreateAccessDatabase()
        {
            // Use late-bound COM interop to create a blank .accdb file via ADOX.Catalog
            Type catalogType = Type.GetTypeFromProgID("ADOX.Catalog")
                ?? throw new Exception(
                    "ADOX.Catalog is not available. Please install Microsoft Access Database Engine Redistributable.");

            object catalog = Activator.CreateInstance(catalogType)!;
            try
            {
                // Use reflection-based invocation for COM (works with .NET Core + COM)
                catalogType.InvokeMember(
                    "Create",
                    BindingFlags.InvokeMethod,
                    null,
                    catalog,
                    new object[] { ConnectionString }
                );
            }
            finally
            {
                // Close the active connection if any
                try
                {
                    object? activeConn = catalogType.InvokeMember(
                        "ActiveConnection",
                        BindingFlags.GetProperty,
                        null,
                        catalog,
                        null
                    );
                    if (activeConn != null)
                    {
                        activeConn.GetType().InvokeMember(
                            "Close",
                            BindingFlags.InvokeMethod,
                            null,
                            activeConn,
                            null
                        );
                        Marshal.ReleaseComObject(activeConn);
                    }
                }
                catch { /* Connection may already be closed */ }

                Marshal.ReleaseComObject(catalog);
            }
        }

        private static void SeedDroneTypes(OleDbConnection conn)
        {
            string insertType = "INSERT INTO tblDroneTypes (TypeName, Category, Description) VALUES (?, ?, ?)";

            var types = new List<(string, string, string)>
            {
                ("Delivery",      "Logistics",    "Transports packages and cargo to designated locations"),
                ("Surveillance",  "Security",     "Monitors areas with high-resolution camera systems"),
                ("Agricultural",  "Agriculture",  "Disperses fertilizers and monitors crop health"),
                ("Medical",       "Emergency",    "Delivers medical supplies to remote or urgent locations"),
                ("Racing",        "Sport",        "High-speed competitive racing drone")
            };

            foreach (var t in types)
            {
                var cmd = new OleDbCommand(insertType, conn);
                cmd.Parameters.AddWithValue("p1", t.Item1);
                cmd.Parameters.AddWithValue("p2", t.Item2);
                cmd.Parameters.AddWithValue("p3", t.Item3);
                cmd.ExecuteNonQuery();
            }
        }

        private static void SeedData(OleDbConnection conn)
        {
            string insertQuery = @"
                INSERT INTO tblDrones (DroneID, DroneType, BatteryLevel, Coordinates, SpecialAttribute) 
                VALUES (?, ?, ?, ?, ?)";

            var drones = new List<(string, string, int, string, string)>
            {
                ("DEL-01", "Delivery", 85, "10,20", "5.5"),
                ("SUR-99", "Surveillance", 42, "15,30", "4K"),
                ("AGR-05", "Agricultural", 95, "55,10", "Nitrogen"),
                ("MED-11", "Medical", 12, "80,90", "First Aid Kit"),
                ("RAC-07", "Racing", 100, "0,0", "150")
            };

            using (var transaction = conn.BeginTransaction())
            {
                foreach (var d in drones)
                {
                    var cmd = new OleDbCommand(insertQuery, conn, transaction);
                    cmd.Parameters.AddWithValue("p1", d.Item1);
                    cmd.Parameters.AddWithValue("p2", d.Item2);
                    cmd.Parameters.AddWithValue("p3", d.Item3);
                    cmd.Parameters.AddWithValue("p4", d.Item4);
                    cmd.Parameters.AddWithValue("p5", d.Item5);
                    cmd.ExecuteNonQuery();
                }
                transaction.Commit();
            }
        }

        /// <summary>
        /// Loads all drones using OleDbDataAdapter with INNER JOIN to tblDroneTypes.
        /// </summary>
        public static List<Drone> LoadFleet()
        {
            var fleet = new List<Drone>();
            using (var conn = new OleDbConnection(ConnectionString))
            {
                conn.Open();

                // INNER JOIN between tblDrones and tblDroneTypes
                string query = @"
                    SELECT d.DroneID, d.DroneType, d.BatteryLevel, d.Coordinates, 
                           d.SpecialAttribute, t.Category, t.Description
                    FROM tblDrones d
                    INNER JOIN tblDroneTypes t ON d.DroneType = t.TypeName";

                // Using OleDbDataAdapter to fill a DataTable
                OleDbDataAdapter adapter = new OleDbDataAdapter(query, conn);
                DataTable dt = new DataTable("Fleet");
                adapter.Fill(dt);

                // Convert DataTable rows to Drone objects
                foreach (DataRow row in dt.Rows)
                {
                    string id      = row["DroneID"].ToString()!;
                    string type    = row["DroneType"].ToString()!;
                    int battery    = Convert.ToInt32(row["BatteryLevel"]);
                    string coords  = row["Coordinates"].ToString()!;
                    string special = row["SpecialAttribute"].ToString()!;

                    Drone? drone = type switch
                    {
                        "Delivery"      => new DeliveryDrone(id, battery, coords, Convert.ToDouble(special)),
                        "Surveillance"  => new SurveillanceDrone(id, battery, coords, special),
                        "Agricultural"  => new AgriculturalDrone(id, battery, coords, special),
                        "Medical"       => new MedicalDrone(id, battery, coords, special),
                        "Racing"        => new RacingDrone(id, battery, coords, Convert.ToInt32(special)),
                        _ => null
                    };

                    if (drone != null) fleet.Add(drone);
                }
            }
            return fleet;
        }

        /// <summary>
        /// Searches fleet using INNER JOIN + LIKE at the database level.
        /// Matches DroneID, DroneType, Category, or Description.
        /// </summary>
        public static List<Drone> SearchFleet(string searchTerm)
        {
            var fleet = new List<Drone>();
            using (var conn = new OleDbConnection(ConnectionString))
            {
                conn.Open();

                // INNER JOIN with search using LIKE across multiple columns
                string query = @"
                    SELECT d.DroneID, d.DroneType, d.BatteryLevel, d.Coordinates, 
                           d.SpecialAttribute, t.Category, t.Description
                    FROM tblDrones d
                    INNER JOIN tblDroneTypes t ON d.DroneType = t.TypeName
                    WHERE d.DroneID LIKE ?
                       OR d.DroneType LIKE ?
                       OR t.Category LIKE ?
                       OR t.Description LIKE ?";

                // Using OleDbDataAdapter with parameterized search
                OleDbDataAdapter adapter = new OleDbDataAdapter(query, conn);
                string wildcard = $"%{searchTerm}%";
                adapter.SelectCommand.Parameters.AddWithValue("p1", wildcard);
                adapter.SelectCommand.Parameters.AddWithValue("p2", wildcard);
                adapter.SelectCommand.Parameters.AddWithValue("p3", wildcard);
                adapter.SelectCommand.Parameters.AddWithValue("p4", wildcard);

                DataTable dt = new DataTable("SearchResults");
                adapter.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    string id      = row["DroneID"].ToString()!;
                    string type    = row["DroneType"].ToString()!;
                    int battery    = Convert.ToInt32(row["BatteryLevel"]);
                    string coords  = row["Coordinates"].ToString()!;
                    string special = row["SpecialAttribute"].ToString()!;

                    Drone? drone = type switch
                    {
                        "Delivery"      => new DeliveryDrone(id, battery, coords, Convert.ToDouble(special)),
                        "Surveillance"  => new SurveillanceDrone(id, battery, coords, special),
                        "Agricultural"  => new AgriculturalDrone(id, battery, coords, special),
                        "Medical"       => new MedicalDrone(id, battery, coords, special),
                        "Racing"        => new RacingDrone(id, battery, coords, Convert.ToInt32(special)),
                        _ => null
                    };

                    if (drone != null) fleet.Add(drone);
                }
            }
            return fleet;
        }

        public static void RegisterDrone(Drone d)
        {
            using (var conn = new OleDbConnection(ConnectionString))
            {
                conn.Open();

                // Ensure the drone type exists in the lookup table
                EnsureDroneTypeExists(conn, d.Type);

                var cmd = new OleDbCommand(@"
                    INSERT INTO tblDrones (DroneID, DroneType, BatteryLevel, Coordinates, SpecialAttribute) 
                    VALUES (?, ?, ?, ?, ?)", conn);

                cmd.Parameters.AddWithValue("p1", d.ID);
                cmd.Parameters.AddWithValue("p2", d.Type);
                cmd.Parameters.AddWithValue("p3", d.BatteryLevel);
                cmd.Parameters.AddWithValue("p4", d.GPS.Coordinates);

                // Get the raw string value back from the derived class
                string specialVal = "";
                if (d is DeliveryDrone del) specialVal = del.PayloadWeight.ToString();
                else if (d is SurveillanceDrone sur) specialVal = sur.CameraResolution;
                else if (d is AgriculturalDrone agr) specialVal = agr.FertilizerType;
                else if (d is MedicalDrone med) specialVal = med.MedicalSupplyType;
                else if (d is RacingDrone rac) specialVal = rac.TopSpeed.ToString();

                cmd.Parameters.AddWithValue("p5", specialVal);

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Ensures a drone type exists in tblDroneTypes before inserting a drone.
        /// </summary>
        private static void EnsureDroneTypeExists(OleDbConnection conn, string typeName)
        {
            var checkCmd = new OleDbCommand(
                "SELECT COUNT(*) FROM tblDroneTypes WHERE TypeName = ?", conn);
            checkCmd.Parameters.AddWithValue("p1", typeName);
            int count = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (count == 0)
            {
                var insertCmd = new OleDbCommand(
                    "INSERT INTO tblDroneTypes (TypeName, Category, Description) VALUES (?, ?, ?)", conn);
                insertCmd.Parameters.AddWithValue("p1", typeName);
                insertCmd.Parameters.AddWithValue("p2", "Custom");
                insertCmd.Parameters.AddWithValue("p3", $"{typeName} drone type");
                insertCmd.ExecuteNonQuery();
            }
        }

        public static void UpdateDroneState(Drone d)
        {
            using (var conn = new OleDbConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new OleDbCommand(
                    "UPDATE tblDrones SET BatteryLevel = ?, Coordinates = ? WHERE DroneID = ?", conn);
                cmd.Parameters.AddWithValue("p1", d.BatteryLevel);
                cmd.Parameters.AddWithValue("p2", d.GPS.Coordinates);
                cmd.Parameters.AddWithValue("p3", d.ID);
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteDrone(string id)
        {
            using (var conn = new OleDbConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new OleDbCommand("DELETE FROM tblDrones WHERE DroneID = ?", conn);
                cmd.Parameters.AddWithValue("p1", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
