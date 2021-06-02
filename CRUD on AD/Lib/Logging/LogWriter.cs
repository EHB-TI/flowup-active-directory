using System;
using System.IO;
using System.Reflection;


public static class LogWriter
{
    private static string m_exePath = string.Empty;

    public static void LogWrite(string logMessage, Type c)
    {
        if (m_exePath.Equals(string.Empty))
        {
            m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        try
        {
            using (StreamWriter w = File.AppendText(m_exePath + "\\Logs\\" + DateTime.Now.ToString("dd-MM-yyyy") +"_log.txt"))
            {
                Log(logMessage, w, c);
            }
        }
        catch (Exception ex)
        {
        }
    }

    public static void Log(string logMessage, TextWriter txtWriter, Type c)
    {
        try
        {
            txtWriter.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Log Entry from {c.FullName}");
            txtWriter.WriteLine(" Message => {0}", logMessage);
            txtWriter.WriteLine("-------------------------------");
        }
        catch (Exception ex)
        {
        }
    }
}