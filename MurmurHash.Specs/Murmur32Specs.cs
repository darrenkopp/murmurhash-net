using Machine.Specifications;
using System.Linq;
using System.Security.Cryptography;

namespace Murmur.Specs
{
    class Murmur32Specs
    {
        [Subject("Murmur32")]
        class given_a_managed_algorithm
        {
            protected static readonly HashExpection Expectation = new HashExpection(32, 0xB0F57EE3);
            protected static uint VerificationHash;

            Establish context = () => VerificationHash = 0;
            Because of = () => VerificationHash = HashVerifier.ComputeVerificationHash(Expectation.Bits, seed => MurmurHash.Create32(seed));
            It should_have_created_a_valid_hash = () => VerificationHash.ShouldEqual(Expectation.Result);
        }

        [Subject("Murmur32")]
        class given_an_unmanaged_algorithm
        {
            protected static readonly HashExpection Expectation = new HashExpection(32, 0xB0F57EE3);
            protected static uint VerificationHash;

            Establish context = () => VerificationHash = 0;
            Because of = () => VerificationHash = HashVerifier.ComputeVerificationHash(Expectation.Bits, seed => MurmurHash.Create32(seed, managed: false));
            It should_have_created_a_valid_hash = () => VerificationHash.ShouldEqual(Expectation.Result);
        }

        [Subject("Murmur32")]
        class given_a_managed_and_unmanaged_algorithm
        {
            protected static byte[] Input;
            protected static HashAlgorithm Managed;
            protected static HashAlgorithm Unmanaged;
            protected static byte[] ManagedResult;
            protected static byte[] UnmanagedResult;

            Establish context = () =>
            {
                Managed = MurmurHash.Create32();
                Unmanaged = MurmurHash.Create32(managed: false);

                Input = new byte[256 * 8];
                using (var crypto = RNGCryptoServiceProvider.Create())
                    crypto.GetNonZeroBytes(Input);
            };

            Because of = () =>
            {
                ManagedResult = Managed.ComputeHash(Input);
                UnmanagedResult = Unmanaged.ComputeHash(Input);
            };

            It should_have_generated_the_same_hash = () => ManagedResult.SequenceEqual(UnmanagedResult).ShouldBeTrue();

            Cleanup cleanup = () =>
            {
                Managed.Dispose();
                Unmanaged.Dispose();
            };
        }
    }
}
