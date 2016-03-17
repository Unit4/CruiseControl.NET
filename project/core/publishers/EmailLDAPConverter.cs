using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.Core.Publishers
{
    /// <summary>
    /// <para>
    /// Looks up the email address via LDAP.
    /// </para>
    /// </summary>
    /// <title>LDAP Email Converter</title>
    /// <version>1.4.1</version>
    /// <example>
    /// <para>
    /// This will search the LDAP for source control userid "js" , using default settings.
    /// </para>
    /// <code>
    /// &lt;converters&gt;
    /// &lt;ldapConverter domainName="TheCompany" /&gt;
    /// &lt;/converters&gt;
    /// </code>
    /// <para>
    /// This will search the LDAP for source control userid "js" , specifying a user for querying the LDAP.
    /// </para>
    /// <code>
    /// &lt;converters&gt;
    /// &lt;ldapConverter domainName="TheCompany" ldapLogOnUser="LdapQuery"  ldapLogOnPassword="LdapQueryPW" /&gt;
    /// &lt;/converters&gt;
    /// </code>
    /// </example>
    /// <para>
    /// Take the following ldap setup : 
    /// <code>
    /// domain name       : FortKnox
    /// sAMAccountName    : JB
    /// givenName         : James
    /// sn                : Bond
    /// displayName       : James Bond
    /// mail              : James.Bond@fortKnox.com
    /// </code>
    /// Suppose the source control displays the modifying user as 'jb', you need the following in the ldapConverter
    /// <code>
    /// DomainName = FortKnox
    /// LdapQueryField = mail
    /// SourceControlFieldToLdapMapper = SAMAccountName
    /// </code>
    /// Suppose the source control displays the modifying user as 'james bond', you need the following in the ldapConverter
    /// <code>
    /// DomainName = FortKnox
    /// LdapQueryField = mail
    /// SourceControlFieldToLdapMapper = displayName
    /// </code>
    /// </para>
    [ReflectorType("ldapConverter")]
    public class EmailLDAPConverter : IEmailConverter
    {
        private string domainName = string.Empty;
        private string ldap_Mail = "mail";
        private string ldap_QueryField = "MailNickName";
        private string sourceControlFieldToLdapMapper = "SAMAccountName";
        private string ldap_LogOnUser = string.Empty;
        private PrivateString ldap_LogOnPassword = string.Empty;


        /// <summary>
        /// The domain to query for the LDAP service.
        /// </summary>
        /// <version>1.0</version>
        /// <default>n/a</default>
        [ReflectorProperty("domainName", Required = true)]
        public string DomainName
        {
            get { return domainName; }
            set { domainName = value; }
        }

        /// <summary>
        /// The field in the LDAP service to use for mapping the source control userid.
        /// </summary>
        /// <version>1.0</version>
        /// <default>MailNickName</default>
        [ReflectorProperty("ldapQueryField", Required = false)]
        public string LdapQueryField
        {
            get { return ldap_QueryField; }
            set { ldap_QueryField = value; }
        }



        /// <summary>
        /// The field in LDAP to use as lookup reference for the user name of the source control.
        /// </summary>
        /// <version>1.9</version>
        /// <default>SAMAccountName</default>
        [ReflectorProperty("sourceControlFieldToLdapMapper", Required = false)]
        public string SourceControlFieldToLdapMapper
        {
            get { return sourceControlFieldToLdapMapper; }
            set { sourceControlFieldToLdapMapper = value; }

        }


        /// <summary>
        /// Username for logging into the LDAP service.
        /// </summary>
        /// <version>1.0</version>
        /// <default>None</default>
        [ReflectorProperty("ldapLogOnUser", Required = false)]
        public string LdapLogOnUser
        {
            get { return ldap_LogOnUser; }
            set { ldap_LogOnUser = value; }
        }

        /// <summary>
        /// The password to use for connecting to the LDAP service.
        /// </summary>
        /// <version>1.0</version>
        /// <default>None</default>
        [ReflectorProperty("ldapLogOnPassword", typeof(PrivateStringSerialiserFactory), Required = false)]
        public PrivateString LdapLogOnPassword
        {
            get { return ldap_LogOnPassword; }
            set { ldap_LogOnPassword = value; }
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        public EmailLDAPConverter()
        {

        }


        /// <summary>
        /// Apply the conversion from username to email address.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>The email address.</returns>
        public string Convert(string username)
        {
            string ldapPath = @"LDAP://" + domainName;
            string ldapFilter = @"(&(objectClass=user)(" + sourceControlFieldToLdapMapper + "=" + username + "))";
            string[] ldapProperties = { ldap_Mail, ldap_QueryField };

            System.DirectoryServices.DirectoryEntry domain;
            if (ldap_LogOnUser.Length > 0)
            {
                domain = new System.DirectoryServices.DirectoryEntry(ldapPath, ldap_LogOnUser, ldap_LogOnPassword.PrivateValue);
            }
            else
            {
                domain = new System.DirectoryServices.DirectoryEntry(ldapPath);
            }


            System.DirectoryServices.DirectorySearcher searcher = new System.DirectoryServices.DirectorySearcher(domain);
            System.DirectoryServices.SearchResult result;

            searcher.Filter = ldapFilter;
            searcher.PropertiesToLoad.AddRange(ldapProperties);

            result = searcher.FindOne();

            searcher.Dispose();

            // Check the result
            if (result != null)
            {
                return result.Properties[ldap_Mail][0].ToString();
            }
            else
            {
                Core.Util.Log.Debug(string.Format(System.Globalization.CultureInfo.CurrentCulture, "No email adress found for user {0} in domain {1}", username, domainName));
                return null;
            }
        }
    }
}
