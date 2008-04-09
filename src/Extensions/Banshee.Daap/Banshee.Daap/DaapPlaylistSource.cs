// 
// DaapPlaylistSource.cs
//
// Author:
//   Alexander Hixon <hixon.alexander@mediati.org>
//
// Copyright (C) 2008 Alexander Hixon
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
using Mono.Unix;

using Banshee.Collection;
using Banshee.Collection.Database;
using Banshee.Playlist;
using Banshee.Sources;

using DAAP;

namespace Banshee.Daap
{
    public class DaapPlaylistSource : Source, ITrackModelSource
    {
        private DaapSource parent;
        public DAAP.Database Database {
            get { return parent.Database; }
        }
        
        private MemoryTrackListModel track_model;
        
        public DaapPlaylistSource (DAAP.Playlist playlist, DaapSource parent) : base (Catalog.GetString ("Playlist"), playlist.Name, parent.DbId)
        {
            this.parent = parent;
            
            track_model = new MemoryTrackListModel ();
            Properties.SetString ("Icon.Name", "source-playlist");
            
            foreach (Track track in playlist.Tracks) {
                if (parent.TrackMap.ContainsKey (track.Id)) {
                    track_model.Add (parent.TrackMap [track.Id]);
                }
            }
        }
        
        public ArtistListModel ArtistModel {
            get { return null; }
        }
        
        public AlbumListModel AlbumModel {
            get { return null; }
        }
        
        public TrackListModel TrackModel {
            get { return track_model; }
        }

        public bool CanAddTracks {
            get { return false; }
        }
        
        public bool CanRemoveTracks {
            get { return false; }
        }
        
        public bool CanDeleteTracks {
            get { return false; }
        }
        
        public void DeleteSelectedTracks ()
        {
        }
        
        public void RemoveSelectedTracks ()
        {
        }
        
        public void Reload ()
        {
            track_model.Reload ();
        }
        
        public bool HasDependencies {
            get { return false; }
        }
        
        public bool ConfirmRemoveTracks {
            get { return false; }
        }
        
        public bool ShowBrowser {
            get { return false; }
        }
        
        protected override string TypeUniqueId {
            get { return "daap-playlist"; }
        }
    }
}
