using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatbot.Models
{
    public class ConversationData
    {
        //State Management properties
        public bool HasWelcomed { get; set; } = false;

        public bool HasSelectedProduct { get; set; } = false;

        public bool HasSelectedProductSize { get; set; } = false;

        //Data
        public ProductSize SelectedProductSize { get; set; } = ProductSize.SMALL;

        public Category Category { get; set; } = Category.NONE;
    }

    public enum ProductSize
    {
        SMALL = 1,

        MEDIUM = 2,

        LARGE = 3,

        EXTRA_LARGE = 4
    }

    public enum Category
    {
        NONE = 0,

        TIMBER = 1,

        COLOR = 2,

        RENOVATION = 3,

        BATHROOM = 4
    }
}