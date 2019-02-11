using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using EllieMae.Encompass.Client;
using EllieMae.Encompass.BusinessObjects.Users;
using EllieMae.Encompass.Collections;
using EllieMae.Encompass.Reporting;

namespace EncLoanAccessReport
{
    class Program
    {
        // Constants for the program return code
        private const int ResultSuccess = 0;
        private const int ResultFailure = 1;

        // Command-line arguments
        private static string serverUri = null;
        private static string userId = null;
        private static string password = null;
        private static string outputFilePath = null;

        static int Main(string[] args)
        {
            try
            {
                // Parse the command line
                if (!readCommandLineArguments(args))
                    return ResultFailure;

                // Log into the Encompass Server
                using (Session session = authenticate())
                {
                    if (session == null)
                        return ResultFailure;

                    // Generate the user loan access information
                    var userLoanCounts = generateLoanAccessInformation(session);

                    // Dump the output to stdout
                    using (TextWriter output = openOutputStream())
                        writeReport(userLoanCounts, output);
                }

                return ResultSuccess;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Unexpected error: " + ex);
                return ResultFailure;
            }
        }

        // Read the command-line parameters
        static bool readCommandLineArguments(string[] args)
        {
            for (int i = 1; i < args.Length; i += 2)
            {
                switch (args[i - 1].ToLower())
                {
                    case "-u": userId = args[i]; break;
                    case "-p": password = args[i]; break;
                    case "-s": serverUri = args[i]; break;
                    case "-o": outputFilePath = args[i]; break;
                    default:
                        Console.Error.WriteLine("Unknown command line argument '{0}'", args[i - 1]);
                        return false;
                }
            }

            // Verify if minimum parameters were provided
            if (serverUri == null)
            {
                Console.Error.WriteLine("Missing required command-line parameter: -s <serverUri>");
                return false;
            }

            if (userId == null)
            {
                Console.Error.WriteLine("Missing required command-line parameter: -u <userId>");
                return false;
            }

            if (password == null)
            {
                Console.Error.WriteLine("Missing required command-line parameter: -p <password>");
                return false;
            }

            return true;
        }

        // Logs into the Encompass Server
        static Session authenticate()
        {
            try
            {
                Session session = new Session();
                session.Start(serverUri, userId, password);
                return session;
            }
            catch (LoginException ex)
            {
                Console.Error.WriteLine("Login Failed for reason: " + ex.ErrorType);
                return null;
            }
        }

        // Generates the number of loans accessible by each Encompass user
        static Dictionary<string, int> generateLoanAccessInformation(Session session)
        {
            // Create the result set dictionary
            Dictionary<string, int> userLoanAccessCounts = new Dictionary<string, int>();

            // Get the list of all users from the server
            UserList users = session.Users.GetAllUsers();

            foreach (User user in users)
            {
                try
                {
                    // Impersonate this user
                    session.ImpersonateUser(user.ID);

                    // Retrieve a loan cursor with all loans (i.e. no filter applied). We pass an empty
                    // field list since we don't need any actual field data.
                    StringList fields = new StringList();

                    using (LoanReportCursor cursor = session.Reports.OpenReportCursor(fields, null))
                    {
                        // The number of loans in the cursor is the total number this user can access
                        userLoanAccessCounts[user.ID] = cursor.Count;

                        // Show a tick on the console
                        Console.Error.Write(".");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Failed to retrieve loan count for user {0}: {1}", user.ID, ex);
                }
                finally
                {
                    // End the impersonation
                    session.RestoreIdentity();
                }
            }

            // Write an endline to the error console
            Console.Error.WriteLine();

            return userLoanAccessCounts;
        }

        static TextWriter openOutputStream()
        {
            if (outputFilePath == null)
                return Console.Out;
            else
                return new StreamWriter(outputFilePath, false);
        }

        // Writes the results to an output strean
        static void writeReport(Dictionary<string, int> userLoanCounts, TextWriter outputStream)
        {
            userLoanCounts.ToList().ForEach(item => outputStream.WriteLine("{0}\t{1}", item.Key, item.Value));
        }

    }
}
