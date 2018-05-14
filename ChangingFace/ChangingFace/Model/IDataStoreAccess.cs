using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangingFace.Model
{
    public interface IDataStoreAccess
    {
        string SaveFace(string userName, byte[] faceBlob);

        IEnumerable<Face> GetFaces(string userName);

        bool DeleteUser(string userName);

        int GetUserId(string userName);

        int GenerateUserId();

        string GetUserName(int userId);

        IEnumerable<string> GetAllUserNames();
    }
}
