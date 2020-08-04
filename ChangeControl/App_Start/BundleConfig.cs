using System.Web;
using System.Web.Optimization;

namespace ChangeControl
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        // "~/Scripts/jquery-{version}.js"));
                        // "~/tmp/plugins/jquery/jquery.min.js",
                        "~/tmp/plugins/jQuery-3.4.1/jquery-3.4.1.js",
                        // "~/tmp/plugins/jquery/jquery.js",
                        "~/Plugin/jquery.mask.min.js",
                        "~/tmp/plugins/jquery-ui/jquery-ui.min.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                    //   "~/Scripts/bootstrap.js"));
                      "~/tmp/plugins/bootstrap/js/bootstrap.bundle.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/tmp/plugins/fontawesome-free/css/all.css",
                        "~/tmp/plugins/overlayScrollbars/css/OverlayScrollbars.min.css",
                        "~/tmp/dist/css/ionicons.min.css",
                        "~/tmp/plugins/datatables-bs4/css/dataTables.bootstrap4.css",
                        "~/tmp/plugins/daterangepicker/daterangepicker.css",
                        "~/tmp/plugins/icheck-bootstrap/icheck-bootstrap.min.css",
                        "~/tmp/plugins/bootstrap-colorpicker/css/bootstrap-colorpicker.min.css",
                        "~/tmp/plugins/tempusdominus-bootstrap-4/css/tempusdominus-bootstrap-4.min.css",
                        "~/tmp/plugins/select2/css/select2.min.css",
                        "~/tmp/plugins/select2-bootstrap4-theme/select2-bootstrap4.min.css",
                        "~/tmp/plugins/bootstrap4-duallistbox/bootstrap-duallistbox.min.css",
                        "~/Plugin/filepond/filepond.css"
                      ));

            bundles.Add(new ScriptBundle("~/bundles/vue").Include(
                        "~/Scripts/vue.js"));
                        
            bundles.Add(new ScriptBundle("~/bundles/sweetalert").Include(
                        "~/Plugin/SweetAlert/sweetalert-dev.js",
                        "~/Plugin/SweetAlert/sweetalert.min.js"));
                        
            bundles.Add(new ScriptBundle("~/bundles/filepond").Include(
                        "~/Plugin/filepond/filepond.min.js",
                        "~/Plugin/filepond/filepond.jquery.js",
                        "~/Plugin/filepond/filepond-plugin-file-encode.js",
                        "~/Plugin/filepond/filepond-plugin-image-preview.min.js",
                        "~/Plugin/filepond/filepond-plugin-file-validate-size.js",
                        "~/Scripts/Shared/FilePond.js",
                        "~/Scripts/Shared/FilePond_alt.js"));

        }
    }
}
