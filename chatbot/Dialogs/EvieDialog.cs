using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chatbot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace chatbot.Dialogs
{
    public class EvieDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserData> _userDataAccessor;
        private readonly IStatePropertyAccessor<ConversationData> _conversationDataAccessor;

        private static string TOP_LEVEL_WATERFALL_NAME = "INITIAL";
        private static String NUM_PRODUCT_DIALOG_PROMPT_NAME = "NUM_PRODUCT_PROMPT";

        public EvieDialog(UserState userState, ConversationState conversationState)
            : base(nameof(EvieDialog))
        {
            _userDataAccessor = userState.CreateProperty<UserData>("UserData");
            _conversationDataAccessor = conversationState.CreateProperty<ConversationData>("ConversationData");

            var topLevelWaterfallSteps = new WaterfallStep[]
            {
                StartAsync
            };

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                PurchaseOrEnquiryStepAsync,
                ProductTypeStepAsync,
                ProductSizeStepAsync,
                NumberOfProductStepAsync,
                ConfirmOrderStepAsync,
                PlaceOrderStepAsync
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(TOP_LEVEL_WATERFALL_NAME, waterfallSteps));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(NUM_PRODUCT_DIALOG_PROMPT_NAME, NumPizzaValidator));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            // The initial child Dialog to run.
            InitialDialogId = TOP_LEVEL_WATERFALL_NAME;
        }

        private static async Task<DialogTurnResult> StartAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(WaterfallDialog), null, cancellationToken);
        }

        private static async Task<DialogTurnResult> PurchaseOrEnquiryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Do you need to Purchase or want to make and Enquiry?"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Purchase", "Enquiry" }),
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> ProductTypeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["purchaseOrEnquiry"] = ((FoundChoice)stepContext.Result).Value;

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("What type of product would do you want to purchase ?"),
                    Choices = ChoiceFactory.ToChoices(new List<string> {
                        "TIMBER", "RENOVATION", "COLOuR", "BATHROOM"}),
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> ProductSizeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["productType"] = ((FoundChoice)stepContext.Result).Value;

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text($"What size of the {stepContext.Values["productType"]} you wish to buy ?"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Small", "Medium", "Large", "Extra Large" }),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> NumberOfProductStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["productSize"] = ((FoundChoice)stepContext.Result).Value;

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text($"How many {stepContext.Values["productSize"]} {stepContext.Values["productType"]} would you like to purchase ?"),
                Choices = ChoiceFactory.ToChoices(new List<string> { "1", "2", "3", "5", "10", "More" }),
                RetryPrompt = MessageFactory.Text("The value entered must be greater than 0 and less than 100."),
            };

            return await stepContext.PromptAsync(NUM_PRODUCT_DIALOG_PROMPT_NAME, promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmOrderStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["productQuantity"] = (int)stepContext.Result;

            var productQuantity = stepContext.Values["productQuantity"];
            var productSize = stepContext.Values["productSize"];
            var productType = stepContext.Values["productType"];

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Order Preview\n{productQuantity} - {productSize} {productType}"), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Does your order look correct?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> PlaceOrderStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //If user typed No, we want to move back to the beginning
            if (!(bool)stepContext.Result)
            {
                return await stepContext.ReplaceDialogAsync(TOP_LEVEL_WATERFALL_NAME, null, cancellationToken);
            }

            //If user types Yes, we want to place the order

            var newOrder = new Order() { DeliveryMethod = (String)stepContext.Values["purchaseOrEnquiry"] };
            newOrder.OrderedProducts.Add(new Product()
            {
                NumberOfItem = (int)stepContext.Values["productQuantity"],
                ItemType = (String)stepContext.Values["productType"],
                ItemSize = (String)stepContext.Values["productSize"]
            });

            // Make a call to the Order API

            // Save Completed Order in the UserProfile on success
            //var userProfile = await _userDataAccessor.GetAsync(stepContext.Context, null, cancellationToken);

            //userProfile.Orders.Add(newOrder);

            //await _userDataAccessor.SetAsync(stepContext.Context, userProfile, cancellationToken);

            await stepContext.Context.SendActivityAsync("Thank you for your order!");

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static Task<bool> NumPizzaValidator(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 0 && promptContext.Recognized.Value < 10);
        }
    }
}