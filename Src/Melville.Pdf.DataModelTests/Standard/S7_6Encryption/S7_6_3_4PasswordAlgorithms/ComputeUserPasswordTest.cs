﻿using System;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Encryption;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Encryption.Readers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.Builder;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption.S7_6_3_4PasswordAlgorithms
{
    public class ComputeUserPasswordTest
    {
        [Theory]
        [InlineData(2, 128)]
        [InlineData(2, 40)]
        public async Task R3Rc4Ciphers(int V, int keyLengthInBits)
        {
            var de = new ComputeEncryptionDictionary("User", "Owner", V, 3, keyLengthInBits, PdfPermission.None,
                new ComputeOwnerPasswordV3(), new ComputeUserPasswordV3(), new GlobalEncryptionKeyComputerV3());
            var id = new PdfArray(
                new PdfString("12345678901234567890123456789012"),
                new PdfString("12345678901234567890123456789012"));
            var encDict = de.CreateEncryptionDictionary(id);
            var trailer = new PdfDictionary((KnownNames.ID, id), (KnownNames.Encrypt, encDict));
            var handler = await SecurityHandlerFactory.CreateSecurityHandler(trailer, encDict);
            Assert.True(handler.TrySinglePassword(("User", PasswordType.User)));
            
        }
        [Theory]
        [InlineData(true,V2R3128RC4CipherWithBlankUserPasswordFromExampleFile, "", PasswordType.User)] 
        [InlineData(false,V2R3128RC4CipherWithBlankUserPasswordFromExampleFile,
            "WrongPassword1|WrongPassword2", PasswordType.User)]
        [InlineData(false, V4RC4128CiperWithUserAndOwnerPasswords,"", PasswordType.User)]
        [InlineData(false, V4RC4128CiperWithUserAndOwnerPasswords,"WrongPassword", PasswordType.User)]
        [InlineData(true, V4RC4128CiperWithUserAndOwnerPasswords,"User", PasswordType.User)]
        [InlineData(false, V4RC4128CiperWithUserAndOwnerPasswords,"User", PasswordType.Owner)]
        [InlineData(true, V4RC4128CiperWithUserAndOwnerPasswords,"Owner", PasswordType.Owner)]
        [InlineData(false, V4RC4128CiperWithUserAndOwnerPasswords,"Owner", PasswordType.User)]
        public async Task VerifyUserPasswordStream(
            bool succeed,string trailer, string passwords, PasswordType passwordType)
        {
            var tDict = (PdfDictionary)await trailer.ParseObjectAsync();
            var handler = await  SecurityHandlerFactory.CreateSecurityHandler(
                tDict, await tDict.GetAsync<PdfDictionary>(KnownNames.Encrypt));
            try
            {
                string[] passwords1 = passwords.Split('|');
                await handler.TryInteactiveLogin(
                    new ConstantPasswordSource(passwordType, passwords1));
                Assert.True(succeed);
            }
            catch (Exception)
            {
                if (succeed) throw; // let the exception flow through if we thought it would work;
            }
        }

        private const string V4RC4128CiperWithUserAndOwnerPasswords = @"
              <</Encrypt <<
                    /CF <<
                        /StdCF << 
                            /AuthEvent/DocOpen/CFM/V2/Length 16
                        >>
                      >>
                      /Filter/Standard
                      /Length 128
                      /O <E60E73846B1C9EB09986B2C20DEAEF48BCC2210F75AE640655EDDFF8B67E7DD6>
                      /U<02FFF40521C0DA9426611D95DFB8AFF3>
                      /P -3392
                      /R 4
                      /StmF /StdCF
                      /StrF/StdCF
                      /V 4
                    >> 
                    /ID[<E65C6E5E42EA0B4781D721F1B83D4C4A><051C4F9DB82F3E4A94E07FA703BC5FA5>] >>";

        private const string V2R3128RC4CipherWithBlankUserPasswordFromExampleFile = @"<</Encrypt <</Filter/Standard/V 2/R 3/Length 128/P -3904/O 
               <E600ECC20288AD8B0D64A929C6A83EE2517679AA0218BECEEA8B7986726A8CDB>
            /U <38ACA54678D67C003A8193381B0FA1CC101112131415161718191A1B1C1D1E1F>>> 
              /ID [<1521FBE61419FCAD51878CC5D478D5FF> <1521FBE61419FCAD51878CC5D478D5FF> ] >>";
    }
}