﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime;
using System.Diagnostics;

namespace QuickFASTDotNet
{
    namespace Examples
    {
        public class PerformanceTestDotNet
        {
            string templateFileName_;
            System.IO.FileStream templateFile_;
            string fastFileName_;
            System.IO.FileStream fastFile_;
            string performanceFileName_;
            System.IO.FileStream performanceFileStream_;
            System.IO.TextWriter performanceFile_;

            bool resetOnMessage_;
            bool strict_;
            uint loop_;
            uint limit_;
            uint duplicate_;


            public PerformanceTestDotNet()
            {
                templateFileName_ = null;
                fastFileName_ = null;
                performanceFileName_ = null;
                loop_ = 1;
                limit_ = 0;
                duplicate_ = 0;
            }


            //~PerformanceTestDotNet()
            //{
            //    if (templateFile_ != null && templateFile_ != System.IO.FileStream.Null)
            //    {
            //        templateFile_.Close();
            //    }
            //    if (templateFile_ != null && fastFile_ != System.IO.FileStream.Null)
            //    {
            //        fastFile_.Close();
            //    }
            //    if (performanceFileName_ != null && performanceFileStream_ != System.IO.FileStream.Null)
            //    {
            //        performanceFileStream_.Close();

            //    }
            //}


            public void usage()
            {
                System.Console.WriteLine("Usage of this program");
                System.Console.WriteLine("  -t file     : Template file (required)");
                System.Console.WriteLine("  -f file     : FAST Message file (required)");
                System.Console.WriteLine("  -p file     : File to which performance measurements are written. (default standard output)");
                System.Console.WriteLine("  -c count    : repeat the test 'count' times");
                System.Console.WriteLine("  -r          : Toggle 'reset decoder on every message' (default false).");
                System.Console.WriteLine("  -s          : Toggle 'strict decoding rules' (default true).");
                System.Console.WriteLine("  -i count  : Interpret messages count times instead of just counting them.");
                System.Console.WriteLine("  -l limit    : Process only the first 'limit' messages.");
            }



            public bool init(ref string[] args)
            {
                bool readTemplateName = false;
                bool readSourceName = false;
                bool readPerfName = false;
                bool readCount = false;
                bool readLimit = false;
                bool readDuplicate = false;
                bool ok = true;

                foreach (string opt in args)
                {
                    if (readTemplateName)
                    {
                        templateFileName_ = opt;
                        readTemplateName = false;
                    }
                    else if (readSourceName)
                    {
                        fastFileName_ = opt;
                        readSourceName = false;
                    }
                    else if (readPerfName)
                    {
                        performanceFileName_ = opt;
                        readPerfName = false;
                    }
                    else if (readCount)
                    {
                        loop_ = System.Convert.ToUInt32(opt);
                        readCount = false;
                    }
                    else if (readLimit)
                    {
                      limit_ = System.Convert.ToUInt32(opt);
                      readLimit = false;
                    }
                    else if (readDuplicate)
                    {
                      duplicate_ = System.Convert.ToUInt32(opt);
                      readDuplicate = false;
                    }
                    else if (opt == "-r")
                    {
                      resetOnMessage_ = !resetOnMessage_;
                    }
                    else if (opt == "-s")
                    {
                      strict_ = !strict_;
                    }
                    else if (opt == "-t")
                    {
                      readTemplateName = true;
                    }
                    else if (opt == "-f")
                    {
                      readSourceName = true;
                    }
                    else if (opt == "-p")
                    {
                      readPerfName = true;
                    }
                    else if (opt == "-c")
                    {
                      readCount = true;
                    }
                    else if (opt == "-h")
                    {
                      ok = false;
                    }
                    else if (opt == "-i")
                    {
                      readDuplicate = true;
                    }
                    else if (opt == "-l")
                    {
                      readLimit = true;
                    }
                }

                try
                {
                    if (templateFileName_ == null)
                    {
                        ok = false;
                        System.Console.WriteLine("ERROR: -t [templatefile] option is required.");
                    }
                    if (ok)
                    {
                        try
                        {

                            templateFile_ = new System.IO.FileStream(templateFileName_, System.IO.FileMode.Open);
                        }
                        catch (System.IO.IOException iex)
                        {
                            ok = false;
                            System.Console.WriteLine("ERROR: Can't open template file: {0}", templateFileName_);
                            System.Console.WriteLine(iex.ToString());
                        }
                    }
                    if (fastFileName_ == null)
                    {
                        ok = false;
                        System.Console.WriteLine("ERROR: -f [FASTfile] option is required.");
                    }
                    if (ok)
                    {
                        try
                        {
                            fastFile_ = new System.IO.FileStream(fastFileName_, System.IO.FileMode.Open);
                        }
                        catch (System.IO.IOException iex)
                        {
                            ok = false;
                            System.Console.WriteLine("ERROR: Can't open FAST Message file: {0}", fastFileName_);
                            System.Console.WriteLine(iex.ToString());
                        }
                    }
                    if (ok && performanceFileName_ != null)
                    {
                        try
                        {
                            performanceFileStream_ = new System.IO.FileStream(performanceFileName_, System.IO.FileMode.OpenOrCreate);
                            performanceFile_ = new System.IO.StreamWriter(performanceFileStream_);
                        }

                        catch (System.IO.IOException iex)
                        {
                            ok = false;
                            System.Console.WriteLine("ERROR: Can't open performance output file: {0}", performanceFileName_);
                            System.Console.WriteLine(iex.ToString());
                        }
                    }
                    else
                    {
                        performanceFile_ = System.Console.Out;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    ok = false;
                }

                if (!ok)
                {
                    this.usage();
                }
                return ok;
            }


            public int parseTemplates(ref QuickFASTDotNet.Codecs.TemplateRegistry templateRegistry)
            {
                try
                {
                    Console.WriteLine("Parsing templates");
                    Console.Out.Flush();
                    QuickFASTDotNet.Stopwatch parseTimer = new QuickFASTDotNet.Stopwatch();
                    templateRegistry = QuickFASTDotNet.Codecs.TemplateRegistry.Parse(templateFile_);
                    parseTimer.Stop();
                    ulong parseLapse = parseTimer.ElapsedMilliseconds;

                    uint templateCount = templateRegistry.Size;
                    performanceFile_.Write("Parsed ");
                    performanceFile_.Write(templateCount);
                    performanceFile_.Write(" templates in ");
                    performanceFile_.Write(parseLapse.ToString("F3"));
                    performanceFile_.WriteLine(" milliseconds.");
                    performanceFile_.Write(" milliseconds. [");
                    performanceFile_.Write("{0:F3} msec/template. = ", (double)parseLapse / (double)templateCount);
                    performanceFile_.WriteLine("{0:F0} template/second.]", (double)1000 * (double)templateCount / (double)parseLapse);
                    performanceFile_.Flush();

                 }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                return 0;
            }


            public int decodeMessages(QuickFASTDotNet.Codecs.TemplateRegistry templateRegistry)
            {
                try
                {
                    for (ulong nPass = 0; nPass < loop_; ++nPass)
                    {
                        System.Console.WriteLine("Decoding input; pass {0} of {1}", nPass + 1, loop_);
                        System.Console.Out.Flush();
                        MessageInterpreter interpreter = new MessageInterpreter(limit_, duplicate_);

                        fastFile_.Seek(0, System.IO.SeekOrigin.Begin);
                        QuickFASTDotNet.Codecs.SynchronousDecoder decoder = new QuickFASTDotNet.Codecs.SynchronousDecoder(templateRegistry, fastFile_);
                        decoder.ResetOnMessage = resetOnMessage_;
                        decoder.Strict = strict_;

                        QuickFASTDotNet.Codecs.MessageReceivedDelegate handlerDelegate;
                        handlerDelegate = new QuickFASTDotNet.Codecs.MessageReceivedDelegate(interpreter.MessageReceived);

                        GCSettings.LatencyMode = GCLatencyMode.LowLatency;

                        QuickFASTDotNet.Stopwatch decodeTimer = new QuickFASTDotNet.Stopwatch();
                        { //PROFILE_POINT("Main");

                            decoder.Decode(handlerDelegate);

                        }//PROFILE_POINT
                        decodeTimer.Stop();
                        ulong decodeLapse = decodeTimer.ElapsedMilliseconds;

                        GCSettings.LatencyMode = GCLatencyMode.Interactive;

                        ulong messageCount = interpreter.getMessageCount();
#if DEBUG
                        performanceFile_.Write("[debug] ");
#endif // DEBUG
                        performanceFile_.Write("Decoded {0} messages in {1}  milliseconds. [", messageCount, decodeLapse);
                        performanceFile_.Write("{0:F3} msec/message. = ", (double)decodeLapse / (double)messageCount);
                        performanceFile_.WriteLine("{0:F3} messages/second]", (double)1000 * (double)messageCount / (double)decodeLapse);

                        //performanceFile_.WriteLine("Decoder time is {0} milliseconds", decoder.DecodeTime);

                        performanceFile_.Flush();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                return 0;
            }



            public int run()
            {
                int result = 0;
                QuickFASTDotNet.Codecs.TemplateRegistry templateRegistry = null;
                result += parseTemplates(ref templateRegistry);
                System.Console.WriteLine("Running second run so JIT will have already run");
                result += parseTemplates(ref templateRegistry);

                result += decodeMessages(templateRegistry);
                System.Console.WriteLine("Running second run so JIT will have already run");
                result += decodeMessages(templateRegistry);

                return result;
            }


            static int Main(string[] args)
            {
                {
                    PerformanceTestDotNet application = new PerformanceTestDotNet();
                    if (application.init(ref args))
                    {
                        application.run();
                    }
                    else
                    {
                        System.Console.WriteLine("ERROR: Failed initialization.");
                    }
                }
                return 0;
            }
        }
    }
}
