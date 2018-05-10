using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace dnscrypt_blacklist_generator
{
    class Program
    {
        static string blackListedDomainsFile;
        static string whiteListedDomainsFile;
        static string blackListedDomainsSourceURLs;
        static string whiteListedDomainsSourceURLs;
        static string dnscryptProxyServiceName;
        static bool restartServiceOnCompletion;
        static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            log.Info("Process started.");
            try
            {
                blackListedDomainsFile = ConfigurationManager.AppSettings["BlackListedDomainsFile"];
                whiteListedDomainsFile = ConfigurationManager.AppSettings["WhiteListedDomainsFile"];
                blackListedDomainsSourceURLs = ConfigurationManager.AppSettings["BlackListedDomainsSourceURLs"];
                whiteListedDomainsSourceURLs = ConfigurationManager.AppSettings["WhiteListedDomainsSourceURLs"];
                dnscryptProxyServiceName = ConfigurationManager.AppSettings["DNSCryptProxyServiceName"];
                restartServiceOnCompletion = bool.Parse(ConfigurationManager.AppSettings["RestartServiceOnCompletion"]);
                log.Info("Application configuration loaded successfully.");
            }
            catch(Exception ex)
            {
               log.Error("Could not load application configuration: " + ex.Message + " - " + ex.InnerException);
                return;
            }

            ConcurrentBag<string> blackListedDomainsSources = new ConcurrentBag<string>();
            foreach (string blackListedDomainsSourceURL in blackListedDomainsSourceURLs.Split('|'))
            {
                if (!string.IsNullOrWhiteSpace(blackListedDomainsSourceURL))
                {
                    blackListedDomainsSources.Add(blackListedDomainsSourceURL);
                }
            }

            ConcurrentBag<string> whiteListedDomainsSources = new ConcurrentBag<string>();
            foreach (string whiteListedDomainsSourceURL in whiteListedDomainsSourceURLs.Split('|'))
            {
                if (!string.IsNullOrWhiteSpace(whiteListedDomainsSourceURL))
                {
                    whiteListedDomainsSources.Add(whiteListedDomainsSourceURL);
                }
            }

            ConcurrentBag<string> blackListedDomains = new ConcurrentBag<string>();
            Task loadBlackListedDomainsTask = Task.Factory.StartNew(() =>
            {
                if (File.Exists(blackListedDomainsFile))
                {
                    try
                    {
                        string blackListedDomainsFileContent = File.ReadAllText(blackListedDomainsFile);
                        using (StringReader stringReader = new StringReader(blackListedDomainsFileContent))
                        {
                            string line;
                            while ((line = stringReader.ReadLine()) != null)
                            {
                                string blackListedDomain = line.Trim().Split('#').FirstOrDefault().Replace("0.0.0.0", "").Replace("127.0.0.1", "").Trim().Replace("||", "").Replace("^", "").Replace(Environment.NewLine, "");
                                if (blackListedDomain.Length > 0 && !blackListedDomain.StartsWith("#") && !blackListedDomain.StartsWith("[") && !blackListedDomain.StartsWith("!") && !blackListedDomain.StartsWith("127.0.0.1") && !blackListedDomain.StartsWith("localhost") && !blackListedDomain.StartsWith("255.255.255.255") && !blackListedDomain.StartsWith("::1") && !blackListedDomain.StartsWith("0:0:0:0:0:0:0:1") && !blackListedDomain.StartsWith("fe80"))
                                {
                                    blackListedDomains.Add(blackListedDomain);
                                }
                            }
                        }

                        log.Info("Loaded " + blackListedDomains.Count.ToString() + " existing blacklisted domains from file: " + blackListedDomainsFile);
                    }
                    catch (Exception ex)
                    {
                       log.Error("Error loading existing blacklisted domains from file: " + ex.Message + " - " + ex.InnerException);
                    }
                }

                if (blackListedDomainsSources.Count > 0)
                {
                    Parallel.ForEach(blackListedDomainsSources, blackListedDomainsSource =>
                    {
                        try
                        {
                            WebClient webClient = new WebClient();
                            string blackListedDomainsFile = webClient.DownloadString(blackListedDomainsSource);

                            using (StringReader stringReader = new StringReader(blackListedDomainsFile))
                            {
                                string line;
                                while ((line = stringReader.ReadLine()) != null)
                                {
                                    string blackListedDomain = line.Trim().Split('#').FirstOrDefault().Replace("0.0.0.0", "").Replace("127.0.0.1", "").Trim().Replace("||", "").Replace("^", "").Replace(Environment.NewLine, "");
                                    if (blackListedDomain.Length > 0 && !blackListedDomain.StartsWith("#") && !blackListedDomain.StartsWith("[") && !blackListedDomain.StartsWith("!") && !blackListedDomain.StartsWith("127.0.0.1") && !blackListedDomain.StartsWith("localhost") && !blackListedDomain.StartsWith("255.255.255.255") && !blackListedDomain.StartsWith("::1") && !blackListedDomain.StartsWith("0:0:0:0:0:0:0:1") && !blackListedDomain.StartsWith("fe80"))
                                    {
                                        blackListedDomains.Add(blackListedDomain);
                                    }
                                }
                            }

                            log.Info("Added blacklisted domains from source: " + blackListedDomainsSource);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error adding new blacklisted domains from source " + blackListedDomainsSource + ": " + ex.Message + " - " + ex.InnerException);
                        }
                    });
                }
            });

            ConcurrentBag<string> whiteListedDomains = new ConcurrentBag<string>();
            Task loadWhiteListedDomainsTask = Task.Factory.StartNew(() =>
            {
                if (File.Exists(whiteListedDomainsFile))
                {
                    try
                    {
                        string whiteListedDomainsFileContent = File.ReadAllText(whiteListedDomainsFile);
                        using (StringReader stringReader = new StringReader(whiteListedDomainsFileContent))
                        {
                            string line;
                            while ((line = stringReader.ReadLine()) != null)
                            {
                                string whiteListedDomain = line.Trim().Split('#').FirstOrDefault().Replace("0.0.0.0", "").Replace("127.0.0.1", "").Trim().Replace("||", "").Replace("^", "").Replace(Environment.NewLine, "");
                                if (whiteListedDomain.Length > 0 && !whiteListedDomain.StartsWith("#") && !whiteListedDomain.StartsWith("[") && !whiteListedDomain.StartsWith("!") && !whiteListedDomain.StartsWith("127.0.0.1") && !whiteListedDomain.StartsWith("localhost") && !whiteListedDomain.StartsWith("255.255.255.255") && !whiteListedDomain.StartsWith("::1") && !whiteListedDomain.StartsWith("0:0:0:0:0:0:0:1") && !whiteListedDomain.StartsWith("fe80"))
                                {
                                    whiteListedDomains.Add(whiteListedDomain);
                                }
                            }
                        }

                        log.Info("Loaded " + whiteListedDomains.Count.ToString() + " existing whitelisted domains from file: " + whiteListedDomainsFile);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error loading existing whitelisted domains from file: " + ex.Message + " - " + ex.InnerException);
                    }
                }

                if (whiteListedDomainsSources.Count > 0)
                {
                    Parallel.ForEach(whiteListedDomainsSources, whiteListedDomainsSource =>
                    {
                        try
                        {
                            WebClient webClient = new WebClient();
                            string whiteListedDomainsFile = webClient.DownloadString(whiteListedDomainsSource);

                            using (StringReader stringReader = new StringReader(whiteListedDomainsFile))
                            {
                                string line;
                                while ((line = stringReader.ReadLine()) != null)
                                {
                                    string whiteListedDomain = line.Trim().Split('#').FirstOrDefault().Replace("0.0.0.0", "").Replace("127.0.0.1", "").Trim().Replace("||", "").Replace("^", "").Replace(Environment.NewLine, "");
                                    if (whiteListedDomain.Length > 0 && !whiteListedDomain.StartsWith("#") && !whiteListedDomain.StartsWith("[") && !whiteListedDomain.StartsWith("!") && !whiteListedDomain.StartsWith("127.0.0.1") && !whiteListedDomain.StartsWith("localhost") && !whiteListedDomain.StartsWith("255.255.255.255") && !whiteListedDomain.StartsWith("::1") && !whiteListedDomain.StartsWith("0:0:0:0:0:0:0:1") && !whiteListedDomain.StartsWith("fe80"))
                                    {
                                        whiteListedDomains.Add(whiteListedDomain);
                                    }
                                }
                            }

                            log.Info("Added whitelisted domains from source: " + whiteListedDomainsSource);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error adding new whitelisted domains from source " + whiteListedDomainsSource + ": " + ex.Message + " - " + ex.InnerException);
                        }
                    });
                }
            });

            loadBlackListedDomainsTask.Wait();
            loadWhiteListedDomainsTask.Wait();

            blackListedDomains = new ConcurrentBag<string>(blackListedDomains.Distinct().ToList().OrderByDescending(blackListedDomain => blackListedDomain));
            if (blackListedDomains.Count > 0)
            {
                List<string> header = new List<string>();
                header.Add("#####################################");
                header.Add("# Generated on " + DateTime.Now.ToString("yyyy-MM-dd") + " at " + DateTime.Now.ToString("HH:mm:ss"));
                header.Add("# Number of entries: " + blackListedDomains.Count.ToString());
                if (blackListedDomainsSources.Count > 0)
                {
                    header.Add("#");
                    header.Add("# Sources:");
                    foreach (string blackListSource in blackListedDomainsSources)
                    {
                        header.Add("# " + blackListSource);
                    }
                    header.Add("#");
                }
                header.Add("# Generated by: dnscrypt-blacklist-generator");
                header.Add("# https://www.williampuckering.com");
                header.Add("#####################################");

                try
                {
                    File.WriteAllLines(blackListedDomainsFile, header);
                    File.AppendAllLines(blackListedDomainsFile, blackListedDomains);
                }
                catch(Exception ex)
                {
                    log.Error("Could not generate blacklisted domains file: " + blackListedDomainsFile + ": " + ex.Message + " - " + ex.InnerException);
                }
            }

            whiteListedDomains = new ConcurrentBag<string>(whiteListedDomains.Distinct().ToList().OrderByDescending(whiteListedDomain => whiteListedDomain));
            if (whiteListedDomains.Count > 0)
            {
                List<string> header = new List<string>();
                header.Add("#####################################");
                header.Add("# Generated on " + DateTime.Now.ToString("yyyy-MM-dd") + " at " + DateTime.Now.ToString("HH:mm:ss"));
                header.Add("# Number of entries: " + whiteListedDomains.Count.ToString());
                if (whiteListedDomainsSources.Count > 0)
                {
                    header.Add("#");
                    header.Add("# Sources:");
                    foreach (string whiteListSource in whiteListedDomainsSources)
                    {
                        header.Add("# " + whiteListSource);
                    }
                    header.Add("#");
                }
                header.Add("# Compiled by: dnscrypt-blacklist-generator");
                header.Add("# https://www.williampuckering.com");
                header.Add("#####################################");

                try
                {
                    File.WriteAllLines(whiteListedDomainsFile, header);
                    File.AppendAllLines(whiteListedDomainsFile, whiteListedDomains);
                }
                catch(Exception ex)
                {
                    log.Error("Could not generate whitelisted domains file: " + whiteListedDomainsFile + ": " + ex.Message + " - " + ex.InnerException);
                }
            }

            if (restartServiceOnCompletion)
            {
                try
                {
                    log.Info("Stopping " + dnscryptProxyServiceName + " service...");
                    Process.Start("net", "stop " + dnscryptProxyServiceName).WaitForExit();
                    log.Info(dnscryptProxyServiceName + " service stopped successfully.");
                    log.Info("Starting " + dnscryptProxyServiceName + " service...");
                    Process.Start("net", "start " + dnscryptProxyServiceName).WaitForExit();
                    log.Info(dnscryptProxyServiceName + " service started successfully.");
                }
                catch (Exception ex)
                {
                    log.Error("Error starting or stopping the dnscrypt-proxy service [" + dnscryptProxyServiceName + "]: " + ex.Message + " - " + ex.InnerException);
                }
            }

            log.Info("Process finished.");
        }
    }
}