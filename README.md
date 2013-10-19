An implementation of the [Murmur3](http://code.google.com/p/smhasher/wiki/MurmurHash3) hash for .NET

As of the time this was written, this library supports the 3 main Murmur3 variants: 32-bit hash (x86), 128-bit hash (x86) and 128-bit hash (x64).
The variants are implemented as HashAlgorithm implementations, so that they can be transparently swapped out with any other .NET framework algorithms.
For each algorithm, there is a managed and unmanaged (rather, unsafe) variant that you can pick when creating the algorithm. 

There currently is not a way to force the selection of a 128-bit algorithm right now, but it automatically detects the process type and returns the optimized algorithm.

You can [grab the the binaries off of nuget](https://www.nuget.org/packages/murmurhash/) and there is also a [signed package](https://www.nuget.org/packages/murmurhash-signed/) if that's your thing.

##Example
	byte[] data = Guid.NewGuid().ToByteArray();
	HashAlgorithm murmur128 = MurmurHash.Create128(managed: false); // returns a 128-bit algorithm using "unsafe" code with default seed
	byte[] hash = murmur128.ComputeHash(data);

	// you can also use a seed to affect the hash
	HashAlgorithm seeded128 = MurmurHash.Create128(seed: 3475832); // returns a managed 128-bit algorithm with seed
	byte[] seedResult = murmur128.ComputeHash(data);

#License
Apache 2.0, but only to the extent of the actual code here. I didn't invent the Murmur[3] algorithm nor do I really understand it, I just ported it as best I could to c#. I picked Apache 2.0 because I want others to benefit from the work here, but not necessarily be able to "sell" the library itself (ie the compiled dll), but you are completely fine to use the library in commercial projects.
I'm not a lawyer, so if you see a problem with the license, let me know and I'll fix it.