//
// Torrent.cs
//
// Authors:
//   Alan McGovern alan.mcgovern@gmail.com
//
// Copyright (C) 2006 Alan McGovern
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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using MonoTorrent.BEncoding;
using System.Collections;
using System.Net;

namespace MonoTorrent.Common
{
    /// <summary>
    /// The "Torrent" class for both Tracker and Client should inherit from this
    /// as it contains the fields that are common to both.
    /// </summary>
    public class Torrent
    {
        #region Private Fields

        private BEncodedValue azureusProperties;
        private List<stringCollection> announceUrls;
        private string comment;
        private string createdBy;
        private DateTime creationDate;
        private byte[] ed2k;
        private string encoding;
        internal byte[] infoHash;
        private bool isPrivate;
        private string name;
        private BEncodedList nodes;
        private int pieceLength;
        private Hashes pieces;
        private string publisher;
        private string publisherUrl;
        private byte[] sha1;
        private long size;
        private string source;
        private TorrentFile[] torrentFiles;
        private string torrentPath;

        #endregion Private Fields


        #region Properties

        /// <summary>
        /// The announce URLs contained within the .torrent file
        /// </summary>
        public List<stringCollection> AnnounceUrls
        {
            get { return this.announceUrls; }
        }


        /// <summary>
        /// FIXME: No idea what this is.
        /// </summary>
        public BEncodedValue AzureusProperties
        {
            get { return this.azureusProperties; }
        }


        /// <summary>
        /// The comment contained within the .torrent file
        /// </summary>
        public string Comment
        {
            get { return this.comment; }
        }


        /// <summary>
        /// The optional string showing who/what created the .torrent
        /// </summary>
        public string CreatedBy
        {
            get { return this.createdBy; }
        }


        /// <summary>
        /// The creation date of the .torrent file
        /// </summary>
        public DateTime CreationDate
        {
            get { return this.creationDate; }
        }


        /// <summary>
        /// The optional ED2K hash contained within the .torrent file
        /// </summary>
        public byte[] ED2K
        {
            get { return this.ed2k; }
        }


        /// <summary>
        /// The encoding used by the client that created the .torrent file
        /// </summary>
        public string Encoding
        {
            get { return this.encoding; }
        }


        /// <summary>
        /// The list of files contained within the .torrent which are available for download
        /// </summary>
        public TorrentFile[] Files
        {
            get { return this.torrentFiles; }
        }


        /// <summary>
        /// This is the infohash that is generated by putting the "Info" section of a .torrent
        /// through a ManagedSHA1 hasher.
        /// </summary>
        public byte[] InfoHash
        {
            get { return infoHash; }
        }


        /// <summary>
        /// Shows whether DHT is allowed or not. If it is a private torrent, no peer
        /// sharing should be allowed.
        /// </summary>
        public bool IsPrivate
        {
            get { return isPrivate; }
        }


        /// <summary>
        /// In the case of a single file torrent, this is the name of the file.
        /// In the case of a multi file torrent, it is the name of the root folder.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }


        /// <summary>
        /// FIXME: No idea what this is.
        /// </summary>
        public BEncodedList Nodes
        {
            get { return this.nodes; }
        }


        /// <summary>
        /// The length of each piece in bytes.
        /// </summary>
        public int PieceLength
        {
            get { return pieceLength; }
        }


        /// <summary>
        /// This is the array of hashes contained within the torrent.
        /// </summary>
        public Hashes Pieces
        {
            get { return this.pieces; }
        }


        /// <summary>
        /// The name of the Publisher
        /// </summary>
        public string Publisher
        {
            get { return publisher; }
        }


        /// <summary>
        /// The Url of the publisher of either the content or the .torrent file
        /// </summary>
        public string PublisherUrl
        {
            get { return this.publisherUrl; }
        }


        /// <summary>
        /// The optional SHA1 hash contained within the .torrent file
        /// </summary>
        public byte[] SHA1
        {
            get { return this.sha1; }
        }


        /// <summary>
        /// The total size of all the files that have to be downloaded.
        /// </summary>
        public long Size
        {
            get { return this.size; }
            set { this.size = value; }
        }


        /// <summary>
        /// The source of the .torrent file
        /// </summary>
        public string Source
        {
            get { return source; }
        }


        /// <summary>
        /// This is the path at which the .torrent file is located
        /// </summary>
        public string TorrentPath
        {
            get { return this.torrentPath; }
        }

        #endregion Properties


        #region Constructors

        private Torrent()
        {
			this.announceUrls = new List<stringCollection>();
            this.comment = string.Empty;
            this.createdBy = string.Empty;
            this.creationDate = new DateTime(1970, 1, 1, 0, 0, 0);
            this.encoding = string.Empty;
            this.name = string.Empty;
            this.publisher = string.Empty;
            this.publisherUrl = string.Empty;
            this.source = string.Empty;
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// This method is called internally to read out the hashes from the info section of the
        /// .torrent file.
        /// </summary>
        /// <param name="data">The byte[]containing the hashes from the .torrent file</param>
        private void LoadHashPieces(byte[] data)
        {
            if (data.Length % 20 != 0)
                throw new TorrentException("Invalid infohash detected");

            this.pieces = new Hashes(data, data.Length / 20);
        }


        /// <summary>
        /// This method is called internally to load in all the files found within the "Files" section
        /// of the .torrents infohash
        /// </summary>
        /// <param name="list">The list containing the files available to download</param>
        private void LoadTorrentFiles(BEncodedList list)
        {
            this.torrentFiles = new TorrentFile[list.Count];
            int i = 0;
            long length;
            string path;
            byte[] md5sum;
            byte[] ed2k;
            byte[] sha1;
            StringBuilder sb = new StringBuilder(32);

            foreach (BEncodedDictionary dict in list)
            {
                length = 0;
                path = null;
                md5sum = null;
                ed2k = null;
                sha1 = null;

                foreach (KeyValuePair<BEncodedString, BEncodedValue> keypair in dict)
                {
                    switch (keypair.Key.Text)
                    {
                        case ("sha1"):
                            sha1 = ((BEncodedString)keypair.Value).TextBytes;
                            break;

                        case ("ed2k"):
                            ed2k = ((BEncodedString)keypair.Value).TextBytes;
                            break;

                        case ("length"):
                            length = long.Parse(keypair.Value.ToString());
                            break;

                        case ("path.utf-8"):
                            foreach (BEncodedString str in ((BEncodedList)keypair.Value))
                            {
                                sb.Append(str.Text);
                                sb.Append(Path.DirectorySeparatorChar);
                            }
                            path = sb.ToString(0, sb.Length - 1);
                            sb.Remove(0, sb.Length);
                            break;

                        case ("path"):
                            if (string.IsNullOrEmpty(path))
                            {
                                foreach (BEncodedString str in ((BEncodedList)keypair.Value))
                                {
                                    sb.Append(str.Text);
                                    sb.Append(Path.DirectorySeparatorChar);
                                }
                                path = sb.ToString(0, sb.Length - 1);
                                sb.Remove(0, sb.Length);
                            }
                            break;

                        case ("md5sum"):
                            md5sum = ((BEncodedString)keypair.Value).TextBytes;
                            break;

                        default:
                            break; //FIXME: Log unknown values
                    }
                }

                this.torrentFiles[i++] = new TorrentFile(path, length, Priority.Normal, md5sum, ed2k, sha1);
                this.size += length;
            }

            return;
        }


        /// <summary>
        /// This method is called internally to load the information found within the "Info" section
        /// of the .torrent file
        /// </summary>
        /// <param name="dictionary">The dictionary representing the Info section of the .torrent file</param>
        private void ProcessInfo(BEncodedDictionary dictionary)
        {
            foreach (KeyValuePair<BEncodedString, BEncodedValue> keypair in dictionary)
            {
                switch (keypair.Key.Text)
                {
                    case ("source"):
                        this.source = keypair.Value.ToString();
                        break;

                    case ("sha1"):
                        this.sha1 = ((BEncodedString)keypair.Value).TextBytes;
                        break;

                    case ("ed2k"):
                        this.ed2k = ((BEncodedString)keypair.Value).TextBytes;
                        break;

                    case ("publisher-url.utf-8"):
                        if (keypair.Value.ToString().Length > 0)
                            this.publisherUrl = keypair.Value.ToString();
                        break;

                    case ("publisher-url"):
                        if ((String.IsNullOrEmpty(publisherUrl)) && (keypair.Value.ToString().Length > 0))
                            this.publisherUrl = keypair.Value.ToString();
                        break;

                    case ("publisher.utf-8"):
                        if (keypair.Value.ToString().Length > 0)
                            this.publisher = keypair.Value.ToString();
                        break;

                    case ("publisher"):
                        if ((String.IsNullOrEmpty(publisher)) && (keypair.Value.ToString().Length > 0))
                            this.publisher = keypair.Value.ToString();
                        break;

                    case ("files"):
                        LoadTorrentFiles(((BEncodedList)keypair.Value));
                        break;

                    case ("name.utf-8"):
                        if (keypair.Value.ToString().Length > 0)
                            this.name = keypair.Value.ToString();
                        break;

                    case ("name"):
                        if ((String.IsNullOrEmpty(this.name)) && (keypair.Value.ToString().Length > 0))
                            this.name = keypair.Value.ToString();
                        break;

                    case ("piece length"):
                        this.pieceLength = int.Parse(keypair.Value.ToString());
                        break;
                    case ("pieces"):
                        LoadHashPieces(((BEncodedString)keypair.Value).TextBytes);
                        break;

                    case ("length"):
                        break;      // This is a singlefile torrent

                    case ("private"):
                        this.isPrivate = (keypair.Value.ToString() == "1") ? true : false;
                        break;

                    default:
                        break;
                }
            }

            if (this.torrentFiles == null)   // Not a multi-file torrent
            {
                long length = long.Parse(dictionary["length"].ToString());
                this.size = length;
                string path = this.name;
                byte[] md5 = (dictionary.ContainsKey("md5")) ? ((BEncodedString)dictionary["md5"]).TextBytes : null;
                byte[] ed2k = (dictionary.ContainsKey("ed2k")) ? ((BEncodedString)dictionary["ed2k"]).TextBytes : null;
                byte[] sha1 = (dictionary.ContainsKey("sha1")) ? ((BEncodedString)dictionary["sha1"]).TextBytes : null;

                this.torrentFiles = new TorrentFile[1];
                this.torrentFiles[0] = new TorrentFile(path, length, Priority.Normal, md5, ed2k, sha1);
            }
        }

        #endregion Private Methods


        #region Public Methods

        /// <summary>
        /// Two Torrent instances are considered equal if they have the same infohash
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if they are equal</returns>
        public override bool Equals(object obj)
        {
            Torrent torrent = obj as Torrent;

            if (torrent == null)
                return false;

            return Toolbox.ByteMatch(this.infoHash, torrent.infoHash);
        }


        /// <summary>
        /// Returns the hashcode of the infohash byte[]
        /// </summary>
        /// <returns>int</returns>
        public override int GetHashCode()
        {
            int result = 0;
            for (int i = 0; i < infoHash.Length; i++)
                result ^= infoHash[i];

            return result;
        }


        public override string ToString()
        {
            return this.name;
        }

        #endregion Public Methods


        #region Loading methods

        /// <summary>
        /// This method loads a .torrent file from the specified path.
        /// </summary>
        /// <param name="path">The path to load the .torrent file from</param>
        public static Torrent Load(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            using (Stream s = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return Torrent.Load(s, path);
        }


        /// <summary>
        /// Loads a .torrent from the supplied stream
        /// </summary>
        /// <param name="stream">The stream containing the data to load</param>
        /// <returns></returns>
        public static Torrent Load(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            return Torrent.Load(stream, "");
        }


        /// <summary>
        /// Loads a .torrent file from the specified URL
        /// </summary>
        /// <param name="url">The URL to download the .torrent from</param>
        /// <param name="location">The path to download the .torrent to before it gets loaded</param>
        /// <returns></returns>
        public static Torrent Load(Uri url, string location)
        {
            if (url == null)
                throw new ArgumentNullException("url");
            if (location == null)
                throw new ArgumentNullException("location");

            try
            {
                using (WebClient client = new WebClient())
                    client.DownloadFile(url, location);
            }
            catch
            {
                throw new TorrentException("Could not download .torrent file from the specified url");
            }

            return Torrent.Load(location);
        }


        private static Torrent Load(Stream stream, string path)
        {
            using (BinaryReader reader = new BinaryReader(stream, new UTF8Encoding()))
            {
                try
                {
                    BEncodedDictionary torrentDictionary = (BEncodedDictionary)BEncodedValue.Decode(reader);
                    Torrent t = Torrent.Load(torrentDictionary);
                    t.torrentPath = path;
                    return t;
                }
                catch (BEncodingException ex)
                {
                    throw new TorrentException("Invalid torrent file specified", ex);
                }
            }
        }


        private static Torrent Load(BEncodedDictionary torrentInformation)
        {
            Torrent t = new Torrent();
            foreach (KeyValuePair<BEncodedString, BEncodedValue> keypair in torrentInformation)
            {
                switch (keypair.Key.Text)
                {
                    case ("announce"):
                        t.announceUrls.Add(new stringCollection());
                        t.announceUrls[0].Add(keypair.Value.ToString());
                        break;

                    case ("creation date"):
                        t.creationDate = t.creationDate.AddSeconds(long.Parse(keypair.Value.ToString()));
                        break;

                    case ("nodes"):
                        t.nodes = (BEncodedList)keypair.Value;
                        break;

                    case ("comment.utf-8"):
                        if (keypair.Value.ToString().Length != 0)
                            t.comment = keypair.Value.ToString();       // Always take the UTF-8 version
                        break;                                          // even if there's an existing value

                    case ("comment"):
                        if (String.IsNullOrEmpty(t.comment))
                            t.comment = keypair.Value.ToString();
                        break;

                    case ("publisher-url.utf-8"):                       // Always take the UTF-8 version
                        t.publisherUrl = keypair.Value.ToString();      // even if there's an existing value
                        break;

                    case ("publisher-url"):
                        if (String.IsNullOrEmpty(t.publisherUrl))
                            t.publisherUrl = keypair.Value.ToString();
                        break;

                    case ("azureus_properties"):
                        t.azureusProperties = keypair.Value;
                        break;

                    case ("created by"):
                        t.createdBy = keypair.Value.ToString();
                        break;

                    case ("encoding"):
                        t.encoding = keypair.Value.ToString();
                        break;

                    case ("info"):
                        t.infoHash = new SHA1Managed().ComputeHash(keypair.Value.Encode());
                        t.ProcessInfo(((BEncodedDictionary)keypair.Value));
                        break;

                    case ("name"):                                               // Handled elsewhere
                        break;

                    case ("announce-list"):
                        BEncodedList announces = (BEncodedList)keypair.Value;
                        t.announceUrls = new List<stringCollection>(announces.Count);

                        for (int j = 0; j < announces.Count; j++)
                        {
                            BEncodedList bencodedTier = (BEncodedList)announces[j];
                            List<string> tier = new List<string>(bencodedTier.Count);

                            for (int k = 0; k < bencodedTier.Count; k++)
                                tier.Add(bencodedTier[k].ToString());

                            Toolbox.Randomize<string>(tier);

                            stringCollection collection = new stringCollection(tier.Count);
                            for (int k = 0; k < tier.Count; k++)
                                collection.Add(tier[k]);
                            t.announceUrls.Add(collection);
                        }
                        break;

                    default:
                        break;
                }
            }

            int i = 0;
            long totalSize = 0;
            foreach (TorrentFile file in t.torrentFiles)
            {
                file.StartPieceIndex = i;
                totalSize += file.Length;
                file.EndPieceIndex = (int)(totalSize / t.pieceLength + (totalSize % t.pieceLength > 0 ? 0 : -1));
                i = file.EndPieceIndex;
            }

            return t;
        }

        #endregion Loading methods
    }
}
