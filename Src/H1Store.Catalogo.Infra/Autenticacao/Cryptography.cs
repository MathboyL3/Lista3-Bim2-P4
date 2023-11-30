using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace H1Store.Catalogo.Infra.Autenticacao
{
    public static class Cryptography
    {
        public static string Encrypt(string pass)
        {
            var pass_bytes = Encoding.UTF8.GetBytes(pass);
            var encoded_pass_bytes = SHA512.HashData(pass_bytes);
            return Encoding.UTF8.GetString(encoded_pass_bytes);
        }
    }
}
