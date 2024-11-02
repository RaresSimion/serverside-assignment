using System.Collections.Generic;

namespace assignment.services
{
    public static class JobStatusStore
    {
        public static Dictionary<string, (string status, List<string> imageIds)> JobStatuses =
            new();
    }
}