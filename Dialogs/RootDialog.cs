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

namespace Bot_Application
{
    [LuisModel("4bd72da0-4fdb-44f9-b8dd-3cd8e39f430b", "4c85f24f240e47e9a3918a5c8a24d4c1", domain: "westeurope.api.cognitive.microsoft.com")]
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        [LuisIntent("Recherche Recette")]        
        public async Task GetRecette(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Je traite ta demande et je reviens vers toi dès que j\'ai trouvé la recette parfaite");
            string url = "https://wsmcommerce.intermarche.com/";
            EntityRecommendation Nourriture;
            if (result.TryFindEntity("Nourriture", out  Nourriture)){
                string product = Nourriture.Entity;
                try
                {
                    using (var Client = new HttpClient())
                    {
                        Client.BaseAddress = new Uri(url);
                        Client.DefaultRequestHeaders.Accept.Clear();
                        Client.DefaultRequestHeaders.Add("TokenAuthentification", "32e88d45-0f1a-4d39-b35b-a8469da5ad10");
                        HttpResponseMessage response = await Client.GetAsync($"api/v1/recherche/recette?mot=\"{product}\"");
                        if (response.IsSuccessStatusCode)
                        {
                            var JSON = await response.Content.ReadAsStringAsync();
                            JObject o = JObject.Parse(JSON);
                            //JArray Recettes = (JArray)o["Recettes"];                        
                            var len = Math.Min(10, o["Recettes"].Count());
                            var reply = context.MakeMessage();
                            reply.Text = string.Format("Voici une selection de recettes qui correspondent à ta recherche:");
                            reply.Attachments = new List<Attachment>();
                            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            for (int i = 0; i < len; i++)
                            {
                                int durée = (int)o["Recettes"][i]["DureePreparation"] + (int)o["Recettes"][i]["DureeCuisson"] + (int)o["Recettes"][i]["DureeRepos"];
                                string idRecette = (string)o["Recettes"][i]["IdRecette"];
                                reply.Attachments.Add(
                                    new HeroCard
                                    {
                                        Title = (string)o["Recettes"][i]["Titre"],
                                        Subtitle = durée + " minutes",
                                        Images = new List<CardImage>
                                        {
                                        new CardImage
                                        {
                                            Url=(string)o["Recettes"][i]["ImageUrl"]
                                        }
                                        },
                                        Buttons = new List<CardAction>
                                        {
                                        new CardAction
                                        {
                                        Value = $"https://drive.intermarche.com/1-idp/recette/{idRecette}-idrec",//TODO choisir le bon magasin
                                        Type = Microsoft.Bot.Connector.ActionTypes.OpenUrl,
                                        Title="Voir sur le site"
                                        }
                                        }
                                    }.ToAttachment()
                                );
                            }
                            await context.PostAsync(reply);
                        }
                        else
                        {
                            Console.Write("ça ne marche pas");
                        };
                    }
                }
                catch (Exception)
                {
                    Console.Write("Exception");
                }
            }
        }

        [LuisIntent("FAQ.Ajout.Express")]
        public async Task FAQAjoutExpress(IDialogContext context, LuisResult result)
        {
            var reply = context.MakeMessage();
            reply.Text = string.Format("Tu es pressé ? L’Ajout Express te permet d’ajouter des produits à ton panier en seulement quelques clics. Rien de plus simple, sélectionne cette option lorsque tu es dans ton panier sur le site de courses en ligne.En cliquant sur le bouton ci- dessous, tu accedes directement aux rayons puis aux sous-familles.Tu n’as plus qu’à compléter ton panier.");
            reply.Attachments = new List<Attachment>();
            reply.Attachments.Add(new Attachment()                                
                {
                ContentUrl = "https://img4.hostingpics.net/pics/782644Capture.png",
                ContentType="image/png",
                Name="ajout_express.png"
                }
            );
            await context.PostAsync(reply);
        }

        [LuisIntent("FAQ.Liste.Courses")]
        public async Task FAQListeCourses(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("La liste de courses procure un gain de temps considérable. Elle te donne la possibilité par un simple clic de déposer dans ton panier les articles que tu commandes régulièrement.Pour que tes prochaines commandes soient plus rapides, tu peux créer des listes thématiques. Remplis ton panier avec les articles désirés, clique ensuite sur « Aller en caisse », puis clique sur le lien « Tout ajouter à une liste ». Donne un nom à ta liste et le tour est joué ! Ta liste de courses est enregistrée, tu pourras la réutiliser lors de ta prochaine visite sur notre site.");
        }

        [LuisIntent("FAQ.Consulter.Liste.Courses")]
        public async Task FAQConsulterListeCourses(IDialogContext context, LuisResult result)


        {
            var reply = context.MakeMessage();
            reply.Text = string.Format("Pour consulter ta liste de courses, tu dois être connecté à ton compte. Tu pourras alors consulter ta liste de courses directement en cliquant ci-dessous.");
            reply.Attachments = new List<Attachment>();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;           
            reply.Attachments.Add(
                new HeroCard
                {                                                                                
                    Buttons = new List<CardAction>
                    {
                        new CardAction
                        {
                        Value =  "/mon-compte/mes-listes-de-courses",
                        Type = ActionTypes.OpenUrl,
                        Title="Par ici!"
                        }
                    }


                }.ToAttachment()
            );
            
            await context.PostAsync(reply);
        }

        [LuisIntent("FAQ.Ancienne.Commande")]
        public async Task FAQAncienneCommande(IDialogContext context, LuisResult result)


        {
            var reply = context.MakeMessage();
            reply.Text = string.Format("Il faut que tu te rendes dans ton compte. Tu peux y acceder en cliquant sur le lien ci-dessous 😁. Dans \"Historique de mes commandes\", sélectionne la commande concernée et clique sur \"Transformer en liste \".");
            reply.Attachments = new List<Attachment>();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments.Add(
                new HeroCard
                {
                    Buttons = new List<CardAction>
                    {
                        new CardAction
                        {
                        Value =  "/mon-compte/mes-commandes",
                        Type = ActionTypes.OpenUrl,
                        Title="Mon Compte"
                        }
                    }


                }.ToAttachment()
            );

            await context.PostAsync(reply);
        }

        [LuisIntent("FAQ.Produit.Favori")]
        public async Task FAQProduitFavori(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Tu peux ajouter un produit dans tes favoris en cliquant sur le coeur situé à coté de ce dernier. Tu pourras le retrouver ensuite dans l’onglet « Mon Drive malin ».");
        }

        [LuisIntent("FAQ.Oubli.Mdp")]
        public async Task FAQOubliMdp(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Lors de ta connexion sur le site, clique sur « J’ai oublié mon mot de passe ». Tu recevra un email avec un lien sur lequel il faudra cliquer pour pouvoir renseigner un nouveau mot de passe. Pense à vérifier tes courriers indésirables si tu n’as pas reçu l’email après quelques minutes 😉.");
        }

        [LuisIntent("FAQ.Suppression.Compte")]
        public async Task FAQSuppressionCompte(IDialogContext context, LuisResult result)


        {
            var reply = context.MakeMessage();
            reply.Text = string.Format("Tu peux à tout moment supprimer ton compte en cliquant sur le bouton ci-dessous. Conformémemt à la loi \"Informatique et Liberté\" (art 38, 39 & 40 de la loi Informatiques et Libertés modifiée du 6 juillet 1978), tu disposes d'un droit d'accès, de modification, de rectification et de suppression des données te concernant. Tu peux exercer ce droit en nous contactant par email à l'adresse suivante: intermarche@mousquetaires.com");
            reply.Attachments = new List<Attachment>();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments.Add(
                new HeroCard
                {
                    Buttons = new List<CardAction>
                    {
                        new CardAction
                        {
                        Value =  "/mon-compte/mon-profil",
                        Type = ActionTypes.OpenUrl,
                        Title="Mon Compte"
                        }
                    }


                }.ToAttachment()
            );

            await context.PostAsync(reply);
        }

        [LuisIntent("FAQ.Changement.Magasin")]
        public async Task FAQChangementMagasin(IDialogContext context, LuisResult result)


        {
            var reply = context.MakeMessage();
            reply.Text = string.Format("Si tu souhaites passer commande dans un autre magasin, je t'invite à cliquer sur le bouton ci-dessous. Dans \"Mes magasins\", clique sur \"changer de magasin\" puis entre le code postal du magasin sur lequel tu veux passer commande");
            reply.Attachments = new List<Attachment>();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments.Add(
                new HeroCard
                {
                    Buttons = new List<CardAction>
                    {
                        new CardAction
                        {
                        Value =  "/mon-compte/mes-magasins",
                        Type = ActionTypes.OpenUrl,
                        Title="Mes magasins"
                        }
                    }


                }.ToAttachment()
            );

            await context.PostAsync(reply);
        }

        [LuisIntent("FAQ.Newsletter")]
        public async Task FAQNewsletter(IDialogContext context, LuisResult result)


        {
            var reply = context.MakeMessage();
            reply.Text = string.Format("Tu peux à tout moment modifier tes abonnements pour recevoir ou non nos communications par email, par SMS ou par voie postale en cliquant sur le bouton ci-dessous");
            reply.Attachments = new List<Attachment>();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments.Add(
                new HeroCard
                {
                    Buttons = new List<CardAction>
                    {
                        new CardAction
                        {
                        Value =  "/mon-compte/mon-profil",
                        Type = ActionTypes.OpenUrl,
                        Title="Mon compte"
                        }
                    }


                }.ToAttachment()
            );

            await context.PostAsync(reply);
        }

        [LuisIntent("FAQ.Confirmation.Commande")]
        public async Task FAQConfirmationCommande(IDialogContext context, LuisResult result)


        {
            var reply = context.MakeMessage();
            reply.Text = string.Format("Si ta commande a bien été prise en compte, tu vas recevoir un email de confirmation de commande. Tu peux également te rendre dans ton compte dans la rubrique « Mes commandes en cours » en cliquant ci-dessous");
            reply.Attachments = new List<Attachment>();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments.Add(
                new HeroCard
                {
                    Buttons = new List<CardAction>
                    {
                        new CardAction
                        {
                        Value =  "/mon-compte/mes-commandes",
                        Type = ActionTypes.OpenUrl,
                        Title="Mes commandes"
                        }
                    }


                }.ToAttachment()
            );

            await context.PostAsync(reply);
        }

        [LuisIntent("FAQ.Changement.Horaire")]
        public async Task FAQChangementHoraire(IDialogContext context, LuisResult result)
        {
            var reply = context.MakeMessage();
            reply.Text = string.Format("Si tu souhaites modifier ton horaire de livraison ou de retrait, rend-toi dans ton compte en cliquant sur le bouton ci-dessous. Dans \"Mes commandes en cours\", sélectionne la commande que tu souhaites modifier. Si ta commande est en statut \"en préparation\" il est malheuresement dejà trop tard pour la modifier 😕.");
            reply.Attachments = new List<Attachment>();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments.Add(
                new HeroCard
                {
                    Buttons = new List<CardAction>
                    {
                        new CardAction
                        {
                        Value =  "/mon-compte/mes-commandes",
                        Type = ActionTypes.OpenUrl,
                        Title="Mes commandes"
                        }
                    }


                }.ToAttachment()
            );
            await context.PostAsync(reply);
        }

        [LuisIntent("FAQ.Produit.Manquant")]
        public async Task FAQProduitManquant(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("En cas de produits manquants lors de ta livraison, ton livreur t'en informera et ceux-ci ne te seront pas facturés.");
        }

        [LuisIntent("FAQ.Delai.Livraison")]
        public async Task FAQDelaiLivraison(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Le délai de livraison dépend du planning proposé par ton Intermarché et du créneau horaire que tu auras choisi.");
        }

        [LuisIntent("FAQ.Produit.Introuvable")]
        public async Task FAQProduitIntrouvable(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Si tu souhaites commander un produit mais que celui - ci n'apparaît pas, il est fort probable qu'il ne soit plus disponible.N'hésite pas à revenir régulièrement sur notre site, des réapprovisionnements sont réalisés fréquemment.");
        }

        [LuisIntent("FAQ.Produits.Frais")]
        public async Task FAQProduitsFrais(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Nos véhicules de livraison sont réfrigérés. Ce mode de livraison permet de conserver tous les types de produits (surgelés, frais…) du magasin à ton domicile. \n Si tu choisis le mode Drive, tes produits frais et surgelés sont conservés à la bonne température jusqu’au retrait");
        }

        [LuisIntent("FAQ.Produit.Trad")]
        public async Task FAQProduitTrad(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Sur notre site de courses en ligne, tu peux commander des produits à la coupe comme si tu étais au rayon boucherie, poissonnerie, ou encore fromagerie de ton magasin. De même, de nombreux fruits et légumes te sont proposés. \n Dans ton panier, tu peux ajouter des commentaires sur tes produits dans la rubrique « Commentaires pour le livreur » pour aider ton préparateur à répondre au mieux à tes attentes. Exemple : « Je souhaite des bananes très mûres », « Je préfère des tranches de jambon très fines »...");
        }

        [LuisIntent("FAQ.Difference.Prix")]
        public async Task FAQDifferencePrix(IDialogContext context, LuisResult result)
        {
            var reply = context.MakeMessage();
            reply.Text = string.Format("Les prix sur le site drive.intermarche.com sont les mêmes que les prix en magasin. Si tu constates une différence de prix entre le site et ton point de vente tu peux nous en informer en appelant le numero ci-dessous");
            reply.Attachments = new List<Attachment>();
            reply.Attachments.Add(new Attachment()
            {
                ContentUrl = "https://driveimg1.intermarche.com/fr/Content/images/compte/BannieresSAV.jpg",
                ContentType = "image/png",
                Name = "ajout_express.png"
            }
            );
            await context.PostAsync(reply);
        }

        [LuisIntent("FAQ.Modes.Paiement")]//TODO faire un deuxième saut de ligne avant l'asterisque
        public async Task FAQModesPaiement(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Plusieurs modes de paiement sont disponibles selon le mode de livraison choisi ainsi que le magasin sélectionné:   {Environment.NewLine}-Pour la livraison, auprès de ton livreur et ce par chèque ou carte bancaire*.  {Environment.NewLine}-Pour le retrait, tu peux payer soit pas carte bancaire soit par chèque auprès du personnel du magasin*.  {Environment.NewLine}-Sur notre site, tu peux payer ta commande directement en ligne par carte bancaire*.  {Environment.NewLine}-Enfin, pour les commandes Drive, tu pourras payer directement à la borne*.  {Environment.NewLine}\n  {Environment.NewLine}*Voir conditions avec ton magasin");
        }

        [LuisIntent("FAQ.Montant.Minimum")]
        public async Task FAQMontantMinimum(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Pour connaître le montant minimum d’achat proposé par votre magasin, rendez - vous sur la page d’accueil au niveau du logo (en haut à gauche), cliquez sur le nom de votre magasin puis allez sur « Voir les plannings ».");
        }

        [LuisIntent("FAQ.Sécurité.Transactions")]
        public async Task FAQSecuriteTransaction(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"A chaque étape, ton paiement en ligne est 100% sécurisé! Aucune information ne transite en clair sur le site : le serveur est en mode crypté et toutes les informations sont codées. Le fait de communiquer ton numéro de carte de crédit sur le serveur bancaire au moment du paiement de ta commande est entièrement sécurisé.  {Environment.NewLine}Par ailleurs, tu remarqueras dans ton navigateur internet une adresse commençant par https:// ainsi qu’un cadenas. Intermarché n’a jamais accès à tes coordonnées et ne les conserve en aucun cas sur ses serveurs.");
        }

        [LuisIntent("FAQ.Refus.Paiement")]
        public async Task FAQRefusPaiement(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Si ton paiement est refusé, pas de panique, ton Intermarché te contactera afin de trouver une solution 😉.");
        }

        [LuisIntent("FAQ.Debit.Commande")]
        public async Task FAQDebitCommande(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Pour le règlement par carte de crédit en ligne, le débit est effectué lors de la livraison de la commande à la condition d’avoir obtenu préalablement l’autorisation de débit de ton compte auprès des centres de paiement compétents, faute de quoi, ta commande ne pourra être prise en compte.");
        }
        
        [LuisIntent("FAQ.Demande.Carte")]
        public async Task FAQDemandeCarte(IDialogContext context, LuisResult result)
        {
            var reply = context.MakeMessage();
            reply.Text = string.Format($"Pour bénéficier des avantages liés au programme, il faut adhérer au programme de fidélité Intermarché   {Environment.NewLine}Tu peux obtenir ta carte gratuitement soit:  {Environment.NewLine}-En te rendant à l'accueil de ton magasin  {Environment.NewLine}-En faisant la demande dans ton espace client en cliquant sur le bouton ci-dessous");
            reply.Attachments = new List<Attachment>();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments.Add(
                new HeroCard
                {
                    Buttons = new List<CardAction>
                    {
                        new CardAction
                        {
                        Value =  "/mon-compte/ma-carte-intermarche",
                        Type = ActionTypes.OpenUrl,
                        Title="Ma carte fidelité"
                        }
                    }


                }.ToAttachment()
            );

            await context.PostAsync(reply);
        }

        [LuisIntent("FAQ.Utilisation.Carte")]
        public async Task FAQUtilisationCarte(IDialogContext context, LuisResult result)
        {
            var reply = context.MakeMessage();
            reply.Text = string.Format("Pour utiliser ta carte fidelité sur le site internet, il faut que tu renseignes ton numero de carte dans la rubrique ci-dessous");
            reply.Attachments = new List<Attachment>();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments.Add(
                new HeroCard
                {
                    Buttons = new List<CardAction>
                    {
                        new CardAction
                        {
                        Value =  "/mon-compte/ma-carte-intermarche",
                        Type = ActionTypes.OpenUrl,
                        Title="Ma carte fidelité"
                        }
                    }
                }.ToAttachment()
            );
            await context.PostAsync(reply);
        }

        [LuisIntent("FAQ.Probleme.Affichage")]
        public async Task FAQProblemeAffichage(IDialogContext context, LuisResult result)
        {
            var reply = context.MakeMessage();
            reply.Text = string.Format("Notre site est optimisé pour Internet Explorer 9, Google Chrome, Mozilla Firefox et Safari. Je te conseille vivement de les télécharger et de mettre à jour ton navigateur internet. Si malgré cela tu rencontres un problème spécifique, je t'invite à contacter mes amis humains du support en utilisant le numero ci-dessous.");
            reply.Attachments = new List<Attachment>();
            reply.Attachments.Add(new Attachment()
            {
                ContentUrl = "https://driveimg1.intermarche.com/fr/Content/images/compte/BannieresSAV.jpg",
                ContentType = "image/png",
                Name = "support.png"
            }
            );
            await context.PostAsync(reply);
        }

        [LuisIntent("FAQ.Validation.Commande")]
        public async Task FAQValidationCommande(IDialogContext context, LuisResult result)
        {
            var reply = context.MakeMessage();
            reply.Text = string.Format("Verifie que ton navigateur est compatible avec notre site. Le site est optimisé pour Internet Explorer 9, Google Chrome, Mozilla Firefox et Safari. Je te conseille vivement de les télécharger et de mettre à jour ton navigateur internet. Si malgré cela tu rencontres un problème spécifique, je t'invite à contacter mes amis humains du support en utilisant le numero ci-dessous.");
            reply.Attachments = new List<Attachment>();
            reply.Attachments.Add(new Attachment()
            {
                ContentUrl = "https://driveimg1.intermarche.com/fr/Content/images/compte/BannieresSAV.jpg",
                ContentType = "image/png",
                Name = "support.png"
            }
            );
            await context.PostAsync(reply);
        }

        [LuisIntent("FAQ.Creneau.Horaire")]
        public async Task FAQCreneauHoraire(IDialogContext context, LuisResult result)
        {
            var reply = context.MakeMessage();
            reply.Text = string.Format($"Essaye de te deconnecter et de te reconnecter. Je t'invite également à verifier que ton navigateur internet est bien compatible avec notre site. Le site est optimisé pour Internet Explorer 9, Google Chrome, Mozilla Firefox et Safari. Je te conseille vivement de les télécharger et de mettre à jour ton navigateur internet. Verifie également que le créneau horaire selectionné est bien disponible.  {Environment.NewLine}Si malgré cela le problème persiste, je t'invite à contacter mes amis humains du support en utilisant le numero ci-dessous.  ");
            reply.Attachments = new List<Attachment>();
            reply.Attachments.Add(new Attachment()
            {
                ContentUrl = "https://driveimg1.intermarche.com/fr/Content/images/compte/BannieresSAV.jpg",
                ContentType = "image/png",
                Name = "support.png"
            }
            );
            await context.PostAsync(reply);
        }

        [LuisIntent("None")]
        public async Task Default(IDialogContext context, LuisResult result)
        {
            var reply = context.MakeMessage();
            reply.Text = string.Format("Je suis desolé je ne comprends pas ta demande. Essaye de la retaper en utilisant des mots plus simple. Sinon, tu peux contacter le support en appelant le numero ci-dessous");
            reply.Attachments = new List<Attachment>();
            reply.Attachments.Add(new Attachment()
            {
                ContentUrl = "https://driveimg1.intermarche.com/fr/Content/images/compte/BannieresSAV.jpg",
                ContentType = "image/png",
                Name = "support.png"
            }
            );
            await context.PostAsync(reply);
        }

        [LuisIntent("SmallTalk")]
        public async Task SmallTalk(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var msg = await activity;
            await context.Forward(new QnABot(),Whatever,msg,CancellationToken.None)
        }

    }
}