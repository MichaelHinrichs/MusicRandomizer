﻿using System;

namespace MusicRandomizer
{
    public enum TrackType
    {
        // VS = Multiplayer, Solo = Single Player, News = Inkopolis News
        Unknown = -1,
        VSLobby = 0,
        VSWait, // plays during Lobby matchmaking
        VSIntro,
        VSBackground,
        VSOneMinute,
        VSWinJingle,
        VSWin,
        VSLoseJingle,
        VSLose,
        SoloWorld,
        SoloGateway, // plays when standing on a Kettle
        SoloMission,
        SoloFinalCheckpoint,
        SoloWin,
        SoloLose,
        NewsBackground,
        NewsOutro,
    }

    public static class TrackTypeUtils
    {
        public static string ToUIString(this TrackType type)
        {
            switch (type) {
                case TrackType.VSLobby:
                    return "Plaza Lobby";
                case TrackType.VSWait:
                    return "Matchmaking";
                case TrackType.VSIntro:
                    return "Intro Jingle";
                case TrackType.VSBackground:
                    return "MP Background Music";
                case TrackType.VSOneMinute:
                    return "One Minute";
                case TrackType.VSWinJingle:
                    return "MP Victory Jingle";
                case TrackType.VSWin:
                    return "MP Victory";
                case TrackType.VSLoseJingle:
                    return "MP Defeat Jingle";
                case TrackType.VSLose:
                    return "MP Defeat";
                case TrackType.SoloWorld:
                    return "SP Hub World";
                case TrackType.SoloGateway:
                    return "Kettle";
                case TrackType.SoloMission:
                    return "SP Background Music";
                case TrackType.SoloFinalCheckpoint:
                    return "Final Checkpoint";
                case TrackType.SoloWin:
                    return "SP Victory";
                case TrackType.SoloLose:
                    return "SP Defeat";
                case TrackType.NewsBackground:
                    return "News";
                case TrackType.NewsOutro:
                    return "News Ending";
                default:
                    return "Unknown";
            }
        }

        public static TrackType FileNameToTrackType(string fileName)
        {
            // we use StartsWith because there's a track in the Plaza with VS in the name
            // we also leave out the "S" because for StartDemo it is lowercase *headdesk*
            if (fileName.StartsWith("STRM_V"))
            {
                if (fileName.Contains("StartDemo"))
                {
                    return TrackType.VSIntro;
                }
                else if (fileName.Contains("ast1min")) // intentionally left out "l" because it can be capital or lowercase
                {
                    return TrackType.VSOneMinute;
                }
                else
                {
                    return TrackType.VSBackground;
                }
            }
            else if (fileName.Contains("Mission"))
            {
                if (fileName.Contains("gateway"))
                {
                    return TrackType.SoloGateway;
                }
                else if (fileName.Contains("LastCheck"))
                {
                    return TrackType.SoloFinalCheckpoint;
                }
                else if (fileName.Contains("End"))
                {
                    return TrackType.SoloWin;
                }
                else if (fileName.Contains("fault"))
                {
                    return TrackType.SoloLose;
                }
                else
                {
                    return TrackType.SoloMission;
                }
            }
            else if (!fileName.StartsWith("Jingle") && fileName.Contains("Result"))
            {
                if (fileName.Contains("Lose"))
                {
                    return TrackType.VSLose;
                }
                else
                {
                    return TrackType.VSWin;
                }
            }
            else if (fileName.Contains("Plaza_News"))
            {
                if (fileName.Contains("End"))
                {
                    return TrackType.NewsOutro;
                }
                else
                {
                    return TrackType.NewsBackground;
                }
            }
            else if (fileName.Equals("STRM_Match"))
            {
                return TrackType.VSWait;
            }
            else if (fileName.Equals("STRM_Lobby"))
            {
                return TrackType.VSLobby;
            }
            else if (fileName.Equals("STRM_Win02"))
            {
                return TrackType.VSWinJingle;
            }
            else if (fileName.Equals("STRM_Lose"))
            {
                return TrackType.VSLoseJingle;
            }
            else if (fileName.Equals("STRM_World"))
            {
                return TrackType.SoloWorld;
            }
            else if (fileName.Equals("STRM_SCLive_LastBossIdol_BU"))
            {
                // this track replaced Ink Me Up during the last splatfest's second day
                // before replacing, ensure that the splatfest is ongoing
                int now = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                if (now < 1469350800)
                {
                    return TrackType.VSBackground;
                }
                else
                {
                    // splatfest is over and the amiibo plaza uses this track, so don't replace it
                    return TrackType.Unknown;
                }
            }

            return TrackType.Unknown;
        }
    }
}
