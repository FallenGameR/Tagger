http://stackoverflow.com/questions/4287352/is-there-a-way-for-application-on-windows-64-bit-to-execute-code-both-under-64-bi
Q: I will need to use API SetWindowHook and I want to set hook both for 64 bit and 32 bit applications. 
A: I cannot suggest a better way of doing it but what I can do is give you the source to a simple hook-based tool which does exactly the same kind of thing. Feel free to the bits that are useful to you:

http://www.pretentiousname.com/NoBarTab/NoBarTab_poc3.zip

(If this URL breaks in the future, just go up a level; it'd probably because I've finished it and put a real page up for the tool and its source.)

It's a VS2010 C++ project but should be easy to compile in older IDEs. (Writing this actually put me off using VS2010 any further for now, heh.)

Obviously, if you use it, please rename any window classes and binary names to avoid conflicts with my tool. (Anything with "NoBarTab" in the name.)

FWIW, this is a tool I started writing a few weeks ago but haven't got around to finishing. The hooking part is finished, though. It hooks window creation so that it can, for specific processes, remove tabs from the Windows 7 taskbar. (I hate the way that feature is used by VMware, in particular.) I was going to release the source code anyway when I finished it...

The 32/64-bit hooking part is all done. The only thing I haven't got around to is adding a config UI so you can specify which processes it should care about, but that's not important for what you are doing.

(I should say that the way I remove tabs from the Win7 taskbar is a complete hack and might break with future versions of Windows. There's no documented way to do that so I had to settle on a nasty kludge. The actual hooking code that you'd be interested in is all "proper", though.)

Also, I made it so that almost all of the real logic is within the main 64-bit exe. The 32-bit EXE just exists to install the 32-bit hook DLL and both the 32-bit and 64-bit hook DLLs just post a message to the main 64-bit exe's hidden window. Whether that is suitable for what you're doing I leave to you to decide, but I figure it probably fits with your desire to have everything in one place as much as possible.
I 
