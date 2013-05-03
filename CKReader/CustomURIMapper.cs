using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Windows.Phone.Storage.SharedAccess;

namespace CKReader
{
    // 自定义URL Mapper类，用来接受其他应用发来的附件文件
    class CustomURIMapper : UriMapperBase
    {
        private string tempUri;

        public override Uri MapUri(Uri uri)
        {
            tempUri = uri.ToString();

            // 检查启动时是否带有文件附件
            // Example launch URI: /FileTypeAssociation?fileToken=89819279-4fe0-4531-9f57-d633f0949a19
            if (tempUri.Contains("/FileTypeAssociation"))
            {
                // 带附件启动，获得文件token串
                int fileIDIndex = tempUri.IndexOf("fileToken=") + 10;
                string fileID = tempUri.Substring(fileIDIndex);

                // 将文件token串发给启动页
                return new Uri("/MainPage.xaml?fileToken=" + fileID, UriKind.Relative);
            }

            // 无附件，正常启动应用
            return uri;
        }
    }
}
