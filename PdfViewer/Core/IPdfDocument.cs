using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfViewer.Core
{
    /// <summary>
    /// Represents a PDF document
    /// </summary>
    public interface IPdfDocument
    {
        /// <summary>
        /// Number of pages in the PDF document
        /// </summary>
        int PageCount { get; }

        /// <summary>
        /// Bookmarks stored in this PdfFile
        /// </summary>
        PdfBookmarkCollection Bookmarks { get; }

        /// <summary>
        /// Size of each page in the PDF document.
        /// </summary>
        IList<SizeF> PageSizes { get; }

        /// <summary>
        /// Renders a page of the PDF document to the provided graphics instance.
        /// </summary>
        /// <param name="page">Number of the page to render.</param>
        /// <param name="graphics">Graphics instance to render the page on.</param>
        /// <param name="dpiX">Horizontal DPI.</param>
        /// <param name="dpiY">Vertical DPI.</param>
        /// <param name="bounds">Bounds to render the page in.</param>
        /// <param name="forPrinting">Render the page for printing.</param>
        void Render(int page, Graphics graphics, float dpiX, float dpiY, Rectangle bounds, bool forPrinting);

        /// <summary>
        /// Renders a page of the PDF document to the provided graphics instance.
        /// </summary>
        /// <param name="page">Number of the page to render.</param>
        /// <param name="graphics">Graphics instance to render the page on.</param>
        /// <param name="dpiX">Horizontal DPI.</param>
        /// <param name="dpiY">Vertical DPI.</param>
        /// <param name="bounds">Bounds to render the page in.</param>
        /// <param name="flags">Flags used to influence the rendering.</param>
        void Render(int page, Graphics graphics, float dpiX, float dpiY, Rectangle bounds, PdfRenderFlags flags);
    }
}
