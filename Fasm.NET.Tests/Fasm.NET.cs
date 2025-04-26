/*
* FasmNet.NET
* https://github.com/ZenLulz/FasmNet.NET
*
* Copyright 2013 ZenLulz ~ Jämes Ménétrey
* Released under the MIT license
*
* Date: 2013-07-10
*/

using FasmNET;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Xunit.Assert;

namespace FasmNET.Tests;

public partial class FasmNetTests
{
    /// <summary>
    /// Gets the version of the compiler.
    /// </summary>
    [Fact]
    public void GetVersion1()
    {
        // Act
        var version = FasmNet.GetVersion();

        // Assert
        Assert.Equal(1,  version.Major);
        Assert.Equal(70, version.Minor);
    }

    /// <summary>
    /// Assembles mnemonics and checks all is right.
    /// </summary>
    [Fact]
    public void Assemble()
    {
        // Act
        var asm = FasmNet.Assemble("use32\nretn");

        // Assert
        Assert.Equal(0xc3, asm[0]);
    }

    /// <summary>
    /// Assembles wrong mnemonics and check the error.
    /// </summary>
    [Fact]
    public void AssembleWithError()
    {
        // Arrange
        const string mnemonics = "use32\nretnj";

        // Act
        try
        {
            FasmNet.Assemble(mnemonics);
            Assert.Fail("The above line must throw an error.");
        }
        catch (FasmAssemblerException ex)
        {
            // Assert
            Assert.Equal(2,                             ex.ErrorLine,   "The error does not indicate the correct line.");
            Assert.Equal(6,                             ex.ErrorOffset, "The error does not indicate the correct offset.");
            Assert.Equal(FasmErrors.IllegalInstruction, ex.ErrorCode,   "The kind of the error was not found.");
            Assert.Equal(mnemonics,                     ex.Mnemonics);
        }
    }

    /// <summary>
    /// Assembles 64-bit mnemonics using an array.
    /// </summary>
    [Fact]
    public void Assemble64BitWithArray()
    {
        // Arrange
        var mnemonics = new[]
        {
            "use64",
            "push rax"
        };

        // Act
        var asm = FasmNet.Assemble(mnemonics);

        // Assert
        Assert.Equal(0x50, asm[0]);
    }

    /// <summary>
    /// Assembles a file.
    /// </summary>
    [Fact]
    public void AssembleFile()
    {
        // Arrange
        const string path = "file.asm";

        // Act
        using (var file = File.CreateText(path))
        {
            file.WriteLine("use32");
            file.WriteLine("push eax");
        }

        var asm = FasmNet.AssembleFile(path);
        File.Delete(path);

        // Assert
        Assert.Equal(0x50, asm[0]); }

    /// <summary>
    /// Assembles some files.
    /// </summary>
    [Fact]
    public void AssembleFiles()
    {
        // Arrange
        const string path1 = "file1.asm";
        const string path2 = "file2.asm";

        // Act
        using (var file = File.CreateText(path1))
        {
            file.WriteLine("use32");
            file.WriteLine("push eax");
        }
        using (var file = File.CreateText(path2))
        {
            file.WriteLine("pop eax");
        }

        var asm = FasmNet.AssembleFiles(new[] { path1, path2 });
        File.Delete(path1);
        File.Delete(path2);

        // Assert
        CollectionAssert.AreEqual(new byte[] { 0x50, 0x58 }, asm);
    }

    /// <summary>
    /// Create an instance of the object and checks the methods AddLine, InsertLine.
    /// </summary>
    [Fact]
    public void ObjectAddAndInsertLines()
    {
        // Arrange
        var fasm = new FasmNet();

        // Act
        fasm.AddLine("push eax");
        fasm.AddLine("pop {0}", "eax");
        fasm.InsertLine(0, "use32");
        var asm = fasm.Assemble();

        // Assert
        CollectionAssert.AreEqual(new byte[] { 0x50, 0x58 }, asm);
    }

    /// <summary>
    /// Create an instance of the object and checks the property Mnemonics.
    /// </summary>
    [Fact]
    public void ObjectMnemonics()
    {
        // Arrange
        var fasm = new FasmNet();

        // Act
        fasm.AddLine("push eax");
        fasm.AddLine("retn");

        // Assert
        Assert.Equal($"push eax{Environment.NewLine}retn{Environment.NewLine}", fasm.Mnemonics);
    }

    /// <summary>
    /// Create an instance of the object and checks the method Clear.
    /// </summary>
    [Fact]
    public void ObjectClear()
    {
        // Arrange
        var fasm = new FasmNet();

        // Act
        fasm.AddLine("retn");
        fasm.Clear();
        fasm.AddLine("push eax");
        fasm.AddLine("pop {0}", "eax");
        fasm.InsertLine(0, "use32");
        var asm = fasm.Assemble();

        // Assert
        CollectionAssert.AreEqual(new byte[] { 0x50, 0x58 }, asm);
    }

    /// <summary>
    /// Create an instance of the object and checks the assmble result with an origin.
    /// </summary>
    [Fact]
    public void ObjectAssembleWithOrigin()
    {
        // Arrange
        var fasm = new FasmNet();

        // Act
        fasm.AddLine("use32");
        fasm.AddLine("jmp {0}", 0x2000);
        var asm = fasm.Assemble(new IntPtr(0x1000));

        // Assert
        CollectionAssert.AreEqual(new byte[] { 0xE9, 0xfb, 0x0f, 0x00, 0x00 }, asm);
    }
}