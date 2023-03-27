# MemOps
This is a fully documented library that enhances the process of metaprogramming by abstracting away the most common memory operations and pointer arithmetic in the classes provided with a nod to performance. All classes are public, so you can get as close as you want to the imported Win32 functions.

# Available Classes
With a multitude of classes placed in the hands of the user, working with memory is easier than ever. Here are the most common classes you may find yourself using from this library:
1. BufferedMemoryAddress
2. MemoryOps
3. ProcessOps
4. ProcessAccessRights
5. And the provided extension classes

# Basic Usage: Opening a Process
To begin using the MemOps library on other processes, you will want to start by opening a handle to the process in mind. You can do this functionally with ProcessOps using the PID or an instance of Process, or use the extension method provided to instances of Process. You will need to know what kind of process access rights you want for your program. All of the possible rights are available in the ProcessAccessRights enum, all forwarded uint flags made public from Win32's PROCESS_ACCESS_RIGHTS.

If you don't need to open a handle, then you can skip this part.

# Basic Usage: Using the Win32 Function Abstractions
To use this in a safe code context, create a new BufferedMemoryAddress and go wild with the possibilities. Because of the nod to performance, it's even possible to bypass the object's buffer entirely in the read and write methods.

Usage of the library's abstractions in a safe context comes from users interacting with the library using native integers while unsafe pointers (like void*) are done internally.

If you need to create multiple BufferedMemoryAddresses, this class comes with a builder to make doing this a breeze. No need to pass a handle or set the buffer over and over again; just use the builder.

# Basic Usage: Interacting with the Win32 External Functions Directly
If you choose to not use any of the abstractions provided, interacting with the Win32 functions is still made super easy to work with, mostly thanks to the CsWin32 library provided by Microsoft that uses metadata to generate the most compatible DLLImport signatures trusted by a large amount of developers.

To use ReadProcessMemory and WriteProcessMemory with this class, you will need to use the forwarded Win32 functions in MemoryOps. The signatures are largely the same but are slightly different to provide more functionality to reading and writing.
