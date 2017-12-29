using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Collections.Generic;
using System.Linq;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.Threading;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;

namespace Bot_Application
{
    [Serializable]
    public class QnABot : QnAMakerDialog
    {
        public BasicQnAMakerDialog() : base(new QnAMakerService(new QnAMakerAttribute(Utils.GetAppSetting("d679b92d574b4f7bbe30290d30cb1329"), Utils.GetAppSetting("54d25cae-a84d-4744-8bd4-326a62889822"),  0.5)))
	    {
        }
    }
}