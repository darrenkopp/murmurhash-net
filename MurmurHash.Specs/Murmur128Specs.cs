using Machine.Specifications;
using System.Linq;
using System.Security.Cryptography;

namespace Murmur.Specs
{
    class Murmur128Specs
    {
        [Subject("Murmur128")]
        class given_a_managed_x64_algorithm
        {
            protected static readonly HashExpection Expectation = new HashExpection(128, 0x6384BA69);
            protected static uint VerificationHash;
            Establish context = () => VerificationHash = 0;
            Because of = () => VerificationHash = HashVerifier.ComputeVerificationHash(Expectation.Bits, seed => MurmurHash.Create128(seed, true, AlgorithmPreference.X64));
            It should_have_computed_correct_hash = () => VerificationHash.ShouldEqual(Expectation.Result);
        }

        [Subject("Murmur128")]
        class given_an_unmanaged_x64_algorithm
        {
            protected static readonly HashExpection Expectation = new HashExpection(128, 0x6384BA69);
            protected static uint VerificationHash;
            Establish context = () => VerificationHash = 0;
            Because of = () => VerificationHash = HashVerifier.ComputeVerificationHash(Expectation.Bits, seed => MurmurHash.Create128(seed, false, AlgorithmPreference.X64));
            It should_have_computed_correct_hash = () => VerificationHash.ShouldEqual(Expectation.Result);
        }

        [Subject("Murmur128")]
        class given_a_managed_x86_algorithm
        {
            protected static readonly HashExpection Expectation = new HashExpection(128, 0xB3ECE62A);
            protected static uint VerificationHash;
            Establish context = () => VerificationHash = 0;
            Because of = () => VerificationHash = HashVerifier.ComputeVerificationHash(Expectation.Bits, seed => MurmurHash.Create128(seed, true, AlgorithmPreference.X86));
            It should_have_computed_correct_hash = () => VerificationHash.ShouldEqual(Expectation.Result);
        }

        [Subject("Murmur128")]
        class given_an_unmanaged_x86_algorithm
        {
            protected static readonly HashExpection Expectation = new HashExpection(128, 0xB3ECE62A);
            protected static uint VerificationHash;
            Establish context = () => VerificationHash = 0;
            Because of = () => VerificationHash = HashVerifier.ComputeVerificationHash(Expectation.Bits, seed => MurmurHash.Create128(seed, false, AlgorithmPreference.X86));
            It should_have_computed_correct_hash = () => VerificationHash.ShouldEqual(Expectation.Result);
        }

        [Subject("Murmur128")]
        class given_a_managed_and_unmanaged_algorithm
        {
            protected static byte[] Input;
            protected static HashAlgorithm Managed;
            protected static HashAlgorithm Unmanaged;
            protected static byte[] ManagedResult;
            protected static byte[] UnmanagedResult;

            Establish context = () =>
            {
                Managed = MurmurHash.Create128();
                Unmanaged = MurmurHash.Create128(managed: false);

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
