﻿using System;
using System.Collections.Generic;
using PeNet.Utilities;

namespace PeNet.Structures
{
    /// <summary>
    ///     The resource directory contains icons, mouse pointer, string
    ///     language files etc. which are used by the application.
    /// </summary>
    public class IMAGE_RESOURCE_DIRECTORY : AbstractStructure
    {
        /// <summary>
        ///     Array with the different directory entries.
        /// </summary>
        public readonly List<IMAGE_RESOURCE_DIRECTORY_ENTRY?>? DirectoryEntries;

        /// <summary>
        ///     Create a new IMAGE_RESOURCE_DIRECTORY object.
        /// </summary>
        /// <param name="peFile">A PE file.</param>
        /// <param name="offset">Raw offset to the resource directory.</param>
        /// <param name="resourceDirOffset">Raw offset to the resource directory entries.</param>
        public IMAGE_RESOURCE_DIRECTORY(IRawFile peFile, long offset, long resourceDirOffset)
            : base(peFile, offset)
        {
            DirectoryEntries = ParseDirectoryEntries(resourceDirOffset);
        }

        /// <summary>
        ///     Characteristics.
        /// </summary>
        public uint Characteristics
        {
            get => PeFile.ReadUInt(Offset);
            set => PeFile.WriteUInt(Offset, value);
        }

        /// <summary>
        ///     Time and date stamp.
        /// </summary>
        public uint TimeDateStamp
        {
            get => PeFile.ReadUInt(Offset + 0x4);
            set => PeFile.WriteUInt(Offset + 0x4, value);
        }

        /// <summary>
        ///     Major version.
        /// </summary>
        public ushort MajorVersion
        {
            get => PeFile.ReadUShort(Offset + 0x8);
            set => PeFile.WriteUShort(Offset + 0x8, value);
        }

        /// <summary>
        ///     Minor version.
        /// </summary>
        public ushort MinorVersion
        {
            get => PeFile.ReadUShort(Offset + 0xa);
            set => PeFile.WriteUShort(Offset + 0xa, value);
        }

        /// <summary>
        ///     Number of named entries.
        /// </summary>
        public ushort NumberOfNameEntries
        {
            get => PeFile.ReadUShort(Offset + 0xc);
            set => PeFile.WriteUShort(Offset + 0xc, value);
        }

        /// <summary>
        ///     Number of ID entries.
        /// </summary>
        public ushort NumberOfIdEntries
        {
            get => PeFile.ReadUShort(Offset + 0xe);
            set => PeFile.WriteUShort(Offset + 0xe, value);
        }

        private List<IMAGE_RESOURCE_DIRECTORY_ENTRY?>? ParseDirectoryEntries(long resourceDirOffset)
        {
            if (SanityCheckFailed())
                return null;

            var numEntries = NumberOfIdEntries + NumberOfNameEntries;

            List<IMAGE_RESOURCE_DIRECTORY_ENTRY?> entries = new List<IMAGE_RESOURCE_DIRECTORY_ENTRY?>(numEntries);

            for (var index = 0; index < numEntries; index++)
            {
                try
                {
                    entries.Add(new IMAGE_RESOURCE_DIRECTORY_ENTRY(PeFile, (uint)index * 8 + Offset + 16,
                        resourceDirOffset));
                }
                catch (IndexOutOfRangeException)
                {
                    entries[index] = null;
                }
            }

            return entries;
        }

        private bool SanityCheckFailed()
        {
            // There exists the case that only some second/third stage directories are valid and others
            // are not. For this case try to parse at least the valid ones. In that case
            // accessing properties throws an "IndexOutOfRange" exception.
            // Example (malicious!): 9d5eb5ac899764d5ed30cc93df8d645e598e2cbce53ae7bb081ded2c38286d1e
            try
            {
                if (NumberOfIdEntries + NumberOfNameEntries >= 1000)
                    return true;
            }
            catch (Exception)
            {
                return true;
            }

            return false;
        }
    }
}