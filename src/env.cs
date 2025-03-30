
namespace VsTool
{
    //public
    public class Env
    {
    #region Ctor & Destructor
        /// <summary>
        /// Standard Constructor
        /// </summary>
        public Env(string path)
        {
            m_path = path;

            var p = System.Runtime.InteropServices.Marshal.BindToMoniker(path);
            System.Console.WriteLine("{}", p);

        }

        /// <summary>
        /// Default Destructor
        /// </summary>
        ~Env()
        {
        }
    #endregion

        private string m_path;
    }

}
