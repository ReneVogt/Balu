using System;
using System.IO;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Balu.Emit;

sealed class PdbPathWriterProvider(string pdbPath) : ISymbolWriterProvider
{
    readonly PortablePdbWriterProvider innerProvider = new();

    public ISymbolWriter GetSymbolWriter(ModuleDefinition module, string fileName) =>
        new PdbPathWriter(innerProvider.GetSymbolWriter(module, fileName), pdbPath);

    public ISymbolWriter GetSymbolWriter(ModuleDefinition module, Stream symbolStream) =>
        new PdbPathWriter(innerProvider.GetSymbolWriter(module, symbolStream), pdbPath);

    sealed class PdbPathWriter(ISymbolWriter innerWriter, string pdbPath) : ISymbolWriter
    {
        const int PdbPathOffset = 24;

        public ISymbolReaderProvider GetReaderProvider() => innerWriter.GetReaderProvider();

        public ImageDebugHeader GetDebugHeader()
        {
            var header = innerWriter.GetDebugHeader();
            var entries = new ImageDebugHeaderEntry[header.Entries.Length];
            for (var i = 0; i < entries.Length; i++)
            {
                var entry = header.Entries[i];
                entries[i] = entry.Directory.Type == ImageDebugType.CodeView ? ReplacePdbPath(entry) : entry;
            }
            return new(entries);
        }

        ImageDebugHeaderEntry ReplacePdbPath(ImageDebugHeaderEntry entry)
        {
            if (entry.Data.Length < PdbPathOffset ||
                entry.Data[0] != 'R' || entry.Data[1] != 'S' || entry.Data[2] != 'D' || entry.Data[3] != 'S')
                throw new InvalidDataException("The portable PDB writer returned an invalid CodeView debug header.");

            var pathBytes = Encoding.UTF8.GetBytes(pdbPath);
            var data = new byte[PdbPathOffset + pathBytes.Length + 1];
            Array.Copy(entry.Data, data, PdbPathOffset);
            Array.Copy(pathBytes, 0, data, PdbPathOffset, pathBytes.Length);

            var directory = entry.Directory;
            directory.SizeOfData = data.Length;
            return new(directory, data);
        }

        public void Write(MethodDebugInformation info) => innerWriter.Write(info);
        public void Dispose() => innerWriter.Dispose();
    }
}
