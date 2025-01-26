namespace blastcms.web.Data
{
    public class BlastTenant 
    {
        public string Id { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string CustomerId { get; set; }
        public string ReferenceId { get; set; }
        public string IdentityTenantId { get; set; }
        public string IdentityApplicationId { get; set; }
        public string OpenIdConnectClientId { get; set; }
        public string OpenIdConnectAuthority { get; set; }
        public string OpenIdConnectClientSecret { get; set; }
        public string ChallengeScheme { get; set; }
        public bool AdminTenant { get; set; } = false;
    }
}
