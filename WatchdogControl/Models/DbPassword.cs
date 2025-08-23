using System.Xml.Serialization;
using Utilities;

namespace WatchdogControl.Models
{
    public class DbPassword
    {
        private string _password;
        private byte[] _encryptedPassword;
        private readonly Crypto _crypto;

        public DbPassword()
        {
            var key = new byte[] { 0xDC, 0x21, 0x5B, 0xB9, 0x26, 0xD2, 0xF4, 0x4B, 0x1A, 0xD0, 0x01, 0xF1, 0x25, 0x01, 0x63, 0x4F, 0xD7, 0x80, 0x9F, 0xAB, 0x9D, 0x5C, 0x3F, 0xA1, 0xD0, 0x4B, 0x25, 0x45, 0x4B, 0xD9, 0x77, 0xAF };
            var iv = new byte[] { 0xE0, 0xDC, 0x06, 0xED, 0x8E, 0x2B, 0xAC, 0xB3, 0xA2, 0x3E, 0x54, 0x40, 0xF2, 0x38, 0xB3, 0xE4 };
            _crypto = new Crypto(key, iv);
        }

        /// <summary>Пароль для поключения к БД</summary>
        [XmlIgnore]
        public string Password
        {
            get => _password;
            set
            {
                if (_password == value)
                    return;

                _password = value;

                _encryptedPassword = _crypto.EncryptString(_password);
            }
        }

        /// <summary>Зашифрованный пароль для поключения к БД
        /// (используется при десериализации)</summary>
        public byte[] EncryptedPassword
        {
            get => _encryptedPassword;
            set
            {
                if (_encryptedPassword == value)
                    return;

                _encryptedPassword = value;

                _password = _crypto.DecryptString(_encryptedPassword);
            }
        }

        public override string ToString()
        {
            return Password;
        }
    }
}
