using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RequestRepresentation
{
    public abstract class BaseFileOps
    {
        public abstract void ProcessFile( string path );

        public bool isDir( ref string test_loc )
        {
            return Directory.Exists( test_loc );
        }

        public bool exists( ref string test_loc )
        {
            return File.Exists( test_loc );
        }

        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        public void ProcessDirectoryRecursive( string target_directory )
        {
            // Process the list of files found in the directory.
            string [] fileEntries = Directory.GetFiles( target_directory );
            foreach ( string fileName in fileEntries )
            {
                ProcessFile( fileName );
            }

            // Recurse into subdirectories of this directory.
            string [] subdirectoryEntries = Directory.GetDirectories( target_directory );
            foreach ( string subdirectory in subdirectoryEntries )
            {
                ProcessDirectoryRecursive( subdirectory );
            }
        }

        // Process all files in the directory passed in
        public void ProcessDirectory( string target_directory )
        {
            // Process the list of files found in the directory.
            string [] fileEntries = Directory.GetFiles( target_directory );
            foreach ( string fileName in fileEntries )
            {
                ProcessFile( fileName );
            }
        }

        public void writeToLoc( ref string data, ref string loc, string fname )
        {
            using ( StreamWriter writer = new StreamWriter( loc + fname ) )
            {
                writer.Write( data );
            } // end using
        }
    }
}
