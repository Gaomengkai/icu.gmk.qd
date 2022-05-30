using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using cx = ChaoXing;

namespace qd
{
    class UserFile
    {
        static readonly string filename = "userData.txt";
        static readonly string cxCookie = "cxCookie.json";
        public static async Task ExistOrCreate()
        {
            var userFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), filename);
            if (File.Exists(userFile)) { return; }
            string[] vs = new string[3] { "", "", "a" };
            await File.WriteAllLinesAsync(userFile, vs);
        }
        public static async Task<string[]> GetArrayFromFile()
        {
            var userFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), filename);
            if(!File.Exists(userFile)) { return null; }
            return await File.ReadAllLinesAsync(userFile);
        }
        public static async Task<bool> SetArrayToFile(string username, string password, bool autologin)
        {
            var userFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), filename);
            if (!File.Exists(userFile)) { return false; }
            string[] vs = new string[3] { username, password, "" };
            if (autologin) vs[2] = "a";
            await File.WriteAllLinesAsync(userFile, vs);
            return true;
        }
        public static async Task<cx.Student.LoginParams> GetCxCodeFromFile()
        {
            var userFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), cxCookie);
            if (!File.Exists(userFile)) { throw new FileNotFoundException("没有存储相应文件"); }
            using FileStream openStream = File.OpenRead(userFile);
            var login = await JsonSerializer.DeserializeAsync<cx.Student.LoginParams>(openStream);
            return login;
        }

        public static async Task SetCxCodeToFile(cx.Student.LoginParams login)
        {
            var userFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), cxCookie);
            using FileStream createStream = File.Create(userFile);
            await JsonSerializer.SerializeAsync(createStream, login);
        }
    }
}