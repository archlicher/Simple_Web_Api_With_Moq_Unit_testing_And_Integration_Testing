using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Services.Infrastructure
{
    public interface IUserIdProvider
    {
        string GetUserId();
    }
}
