using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace ConsoleApp3
{
    public class TeacherWorkload
    {
        public string DNAME { get; set; }  
        public string GROUP_NAME { get; set; }
        public int ID_CIU_UNIQUE { get; set; }
        public int ID_CURRICULUM { get; set; }
        public int ID_DISCNAME { get; set; }
        public int ID_STUDY_GROUP { get; set; }
        public string P_KAFEDRA { get; set; }
        public string P_NAME { get; set; }
        public int SEMESTER { get; set; }
    }
    
    public class Curriculum
    {
        public int id_curriculum { get; set; }
        public List<Discipline> disciplineList{ get; set; }
    }

    public class Discipline
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<Teacher> teacherList { get; set; }
    }

    public class Teacher
    {
        public int id_ciu_unique { get; set; }
        public string name { get; set; }
        public string kafedra { get; set; }
        public List<Group> groupList { get; set; }
    }

    public class Group
    {
        public int id { get; set; }
        public string name { get; set; }
    }


    class Program
    {
        private const string ProjectPath = @"C:\Projects\ConsoleApp3";
        private const string SourceJsonPath = @"src.json";
        private const string ResultJsonPath = @"result.json";
        
        static async Task Main()
        {
            // Read the JSON file
            string sourcePath = Path.Combine(ProjectPath, SourceJsonPath);
            List<TeacherWorkload> teacherWorkloadList;

            using (var reader = new StreamReader(sourcePath))
            {
                string json = await reader.ReadToEndAsync();
                teacherWorkloadList = JsonSerializer.Deserialize<List<TeacherWorkload>>(json);
            }

            /*
            // Output results
            var studentPropertyList = typeof(TeacherWorkload).GetProperties();
            foreach (var item in teacherWorkloadList)
            {
                foreach (var property in studentPropertyList)
                {
                    var propertyValue = property.GetValue(item);
                    Console.WriteLine($"{property.Name}: {propertyValue}");
                }
                Console.WriteLine("---------------------------------------");
            }*/
            
            // Data grouping rule "учебный план -> дисциплина -> преподаватель -> группа"
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var groupedArray = teacherWorkloadList?.GroupBy(cg => new { cg.ID_CURRICULUM })
                .Select(c => new Curriculum
                {
                    id_curriculum = c.Key.ID_CURRICULUM,
                    disciplineList = c.GroupBy(dg => new { dg.ID_DISCNAME, dg.DNAME })
                        .Select(d => new Discipline
                        {
                            id = d.Key.ID_DISCNAME, // Assuming id is of the same type as ID_DISCNAME
                            name = d.Key.DNAME,
                            teacherList = d.GroupBy(tg => new { tg.ID_CIU_UNIQUE, tg.P_NAME, tg.P_KAFEDRA })
                                .Select(t => new Teacher
                                {
                                    id_ciu_unique = t.Key.ID_CIU_UNIQUE,
                                    name = t.Key.P_NAME,
                                    kafedra = t.Key.P_KAFEDRA,
                                    groupList = t.GroupBy(grg => new { grg.ID_STUDY_GROUP, grg.GROUP_NAME })
                                        .Select(gr => new Group
                                        {
                                            id = gr.Key.ID_STUDY_GROUP,
                                            name = gr.Key.GROUP_NAME
                                        }).ToList()
                                }).ToList()
                        }).ToList()
                }).ToArray();
            stopwatch.Stop();
            Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
            
            // Write the JSON string to a file
            string resultPath = Path.Combine(ProjectPath, ResultJsonPath);
            string jsonString = JsonSerializer.Serialize(groupedArray, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            });
            await File.WriteAllTextAsync(resultPath, jsonString);
        }
    }
}

