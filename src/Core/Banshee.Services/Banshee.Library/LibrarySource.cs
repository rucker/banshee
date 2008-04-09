//
// LibrarySource.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2005-2007 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;

using Mono.Unix;

using Hyena;
using Hyena.Collections;
using Hyena.Data.Sqlite;

using Banshee.Base;
using Banshee.Sources;
using Banshee.Database;
using Banshee.ServiceStack;
using Banshee.Collection;
using Banshee.Collection.Database;

namespace Banshee.Library
{
    public class LibrarySource : PrimarySource
    {
        public LibrarySource (string label, string name, int order) : base (label, label, name, order)
        {
            Properties.SetString ("GtkActionPath", "/LibraryContextMenu");
            Properties.SetString ("RemoveTracksActionLabel", Catalog.GetString ("Remove From Library"));
            AfterInitialized ();
        }

        public override string BaseDirectory {
            get { return Paths.CachedLibraryLocation; }
        }

        protected override void DeleteTrack (DatabaseTrackInfo track)
        {
            try {
                Banshee.IO.Utilities.DeleteFileTrimmingParentDirectories (track.Uri);
            } catch (System.IO.FileNotFoundException) {
            } catch (System.IO.DirectoryNotFoundException) {
            }
        }

        protected override void AddTrack (DatabaseTrackInfo track)
        {
            // Ignore if already have it
            if (track.PrimarySourceId == DbId)
                return;

            // Move it if its from another library source
            if (track.PrimarySource is LibrarySource) {
                PrimarySource old_primary = track.PrimarySource;
                track.PrimarySource = this;
                track.Save (false);
                old_primary.NotifyTracksChanged ();
                return;
            }

            // Otherwise, copy it
            if (track.PrimarySource is LibrarySource) {
                // queue in importer or something?
            }

            /*try {
                Banshee.IO.Utilities.DeleteFileTrimmingParentDirectories (track.Uri);
            } catch (System.IO.FileNotFoundException) {
            } catch (System.IO.DirectoryNotFoundException) {
            }*/
        }

    }
}
