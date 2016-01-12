using Mozu.Api;
using Mozu.Api.Contracts.AppDev;
using Mozu.Api.Security;
using System;

namespace MozuProductReservationsReport
{
    public class MozuContext
    {
        public static string MozuApplicationId { get; set; }
        public static string MozuApplicationSecret { get; set; }

        private static AppAuthInfo _mozuAppAuthInfo;
        private static bool isAuthed = false;

        public int TenantId = 0;
        public int? SiteId = null;
        public int? MasterCatalogId = null;
        public int? CatalogId = null;
        private ApiContext _apiContext;

        private MozuContext() { }

        public ApiContext GetApiContext(bool acceptCached = true)
        {
            if (acceptCached && _apiContext != null)
            {
                return _apiContext;
            }
            ApiContext apiContext = new ApiContext(tenantId: TenantId, siteId: SiteId, masterCatalogId: MasterCatalogId, catalogId: CatalogId);
            _apiContext = apiContext;

            return apiContext;
        }

        public static bool IsAuthed { get { return isAuthed; } }

        public static void AuthApp(string mozuApplicationId, string mozuApplicationSecret)
        {
            MozuApplicationId = mozuApplicationId;
            MozuApplicationSecret = mozuApplicationSecret;
            MozuConfig.ThrowExceptionOn404 = true;

            // auth with mozu
            _mozuAppAuthInfo = new AppAuthInfo()
            {
                ApplicationId = MozuApplicationId,
                SharedSecret = MozuApplicationSecret
            };
            AppAuthenticator.Initialize(_mozuAppAuthInfo);
            isAuthed = true;
        }

        public static MozuContext GetMozuContext(int tenantId, int? siteId = null, int? masterCatalogId = null, int? catalogId = null)
        {
            if (_mozuAppAuthInfo == null)
            {
                throw new ApplicationException("Call AuthApp() first.");
            }

            MozuContext context = new MozuContext()
            {
                TenantId = tenantId,
                SiteId = siteId,
                MasterCatalogId = masterCatalogId,
                CatalogId = catalogId
            };

            AppAuthenticator.Instance.EnsureAuthTicket();

            return context;
        }
    }
}
