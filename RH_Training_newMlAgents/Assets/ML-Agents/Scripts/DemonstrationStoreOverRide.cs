using System.IO;
using System.Text.RegularExpressions;
using System.IO.Abstractions;
using Google.Protobuf;
using UnityEngine;

namespace MLAgents
{
    /// <summary>
    /// Responsible for writing demonstration data to file.
    /// </summary>
    public class DemonstrationStoreOverRide
    {
        public const int MetaDataBytes = 32; // Number of bytes allocated to metadata in demo file.
        readonly IFileSystem m_FileSystem;
        const string k_DemoDirectory = "Assets/Demonstrations/Meta/";
        const string k_ExtensionType = ".demo";
        public int Dummy = 99;
        public string m_FilePath;
        DemonstrationMetaData MM_MetaData;
        Stream m_Writer;
        float m_CumulativeReward;

        public DemonstrationStoreOverRide(IFileSystem fileSystem)
        {
            if (fileSystem != null)
            {
                m_FileSystem = fileSystem;
            }
            else
            {
                m_FileSystem = new FileSystem();
            }
        }

        /// <summary>
        /// Initializes the Demonstration Store, and writes initial data.
        /// </summary>
        public void Initialize(
            string demonstrationName, BrainParameters brainParameters, string brainName)
        {   Debug.Log("Initialised Demo!!");
            CreateDirectory();
            CreateDemonstrationFile(demonstrationName);
            WriteBrainParameters(brainName, brainParameters);
        }

        /// <summary>
        /// Checks for the existence of the Demonstrations directory
        /// and creates it if it does not exist.
        /// </summary>
        void CreateDirectory()
        {
            if (!m_FileSystem.Directory.Exists(k_DemoDirectory))
            {
                m_FileSystem.Directory.CreateDirectory(k_DemoDirectory);
            }
        }

        /// <summary>
        /// Creates demonstration file.
        /// </summary>
        void CreateDemonstrationFile(string demonstrationName)
        {
            // Creates demonstration file.
            var literalName = demonstrationName;
            m_FilePath = k_DemoDirectory + literalName + k_ExtensionType;
            var uniqueNameCounter = 0;
            while (m_FileSystem.File.Exists(m_FilePath))
            {
                literalName = demonstrationName + "_" + uniqueNameCounter;
                m_FilePath = k_DemoDirectory + literalName + k_ExtensionType;
                uniqueNameCounter++;
            }

            m_Writer = m_FileSystem.File.Create(m_FilePath);
            MM_MetaData = new DemonstrationMetaData { demonstrationName = demonstrationName };
            var metaProto = MM_MetaData.ToProto();
            metaProto.WriteDelimitedTo(m_Writer);
            Debug.Log("Should have written to file..");
        }

        /// <summary>
        /// Writes brain parameters to file.
        /// </summary>
	public void WriteBrainParameters(string brainName, BrainParameters brainParameters)
	//void WriteBrainParameters(string brainName, BrainParameters brainParameters)
        {
            // Writes BrainParameters to file.
            //Debug.Log("#### Wrote META EXP");
            //Debug.Log( brainParameters.ToProto(brainName, false));
            //Debug.Log("Wrote META EXP");

            Debug.Log("#### Wrote META EXP");
            Debug.Log( m_Writer.Seek(MetaDataBytes + 1, 0));
            Debug.Log("Wrote META EXP");
            m_Writer.Seek(MetaDataBytes + 1, 0);
            var brainProto = brainParameters.ToProto(brainName, false);
            brainProto.WriteDelimitedTo(m_Writer);

        }

        /// <summary>
        /// Write AgentInfo experience to file.
        /// </summary>
        public void Record(AgentInfo info)
        {
            // Increment meta-data counters.
            MM_MetaData.numberExperiences++;
            m_CumulativeReward += info.reward;

            if (info.done)
            {
                EndEpisode();
            }

            //Debug.Log("#### Wrote META EXP");
            //Debug.Log( info.ToInfoActionPairProto());
            //Debug.Log("Wrote META EXP");

            // Write AgentInfo to file.
            var agentProto = info.ToInfoActionPairProto();
            agentProto.WriteDelimitedTo(m_Writer);
        }

        /// <summary>
        /// Performs all clean-up necessary
        /// </summary>
        public void Close()
        {
            EndEpisode();
            MM_MetaData.meanReward = m_CumulativeReward / MM_MetaData.numberEpisodes;
            WriteMetadata();
            m_Writer.Close();
            Debug.Log("Closing demostore..");
        }

        /// <summary>
        /// Performs necessary episode-completion steps.
        /// </summary>
        void EndEpisode()
        {
            MM_MetaData.numberEpisodes += 1;
        }

        /// <summary>
        /// Writes meta-data.
        /// </summary>
        public void WriteMetadata()
        {
            var metaProto = MM_MetaData.ToProto();
            var metaProtoBytes = metaProto.ToByteArray();
            m_Writer.Write(metaProtoBytes, 0, metaProtoBytes.Length);
            m_Writer.Seek(0, 0);
            metaProto.WriteDelimitedTo(m_Writer);
        }

        /// <summary>
        /// Removes all characters except alphanumerics from demonstration name.
        /// Shorten name if it is longer than the maxNameLength.
        /// </summary>
        public string SanitizeName(string demoName, int maxNameLength)
        {
            var rgx = new Regex("[^a-zA-Z0-9 -]");
            demoName = rgx.Replace(demoName, "");
            // If the string is too long, it will overflow the metadata.
            if (demoName.Length > maxNameLength)
            {
                demoName = demoName.Substring(0, maxNameLength);
            }
            return demoName;
        }


    }
}
