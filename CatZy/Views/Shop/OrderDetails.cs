@model Catzy.Models.OrderDetailsViewModel
@{
    ViewBag.Title = "Order #" + Model.OrderId;
}


<div class="order-details">
    <h2>Order #@Model.OrderId</h2>
    <p class="order-meta">Date: @Model.CreatedAt</p>
    <p class="order-meta">Name: @Model.FullName</p>
    <p class="order-meta">Address: @Model.Address, @Model.City, @Model.PostalCode</p>
    <p class="order-meta">Payment: @Model.PaymentMethod</p>

    <table class="order-table">
        <thead><tr><th>Product</th><th>Price</th><th>Qty</th><th>Total</th></tr></thead>
        <tbody>
            @foreach (var item in Model.Items)
            {
                <tr>
                    <td>@item.Name</td>
                    <td>@item.UnitPrice</td>
                    <td>@item.Quantity</td>
                    <td>@(item.UnitPrice * item.Quantity)</td>
                </tr>
            }
        </tbody>
        <tfoot>
            <tr><td colspan="3" style="text-align:right">Subtotal:</td><td>@Model.Subtotal</td></tr>
            <tr><td colspan="3" style="text-align:right">Shipping:</td><td>@Model.Shipping</td></tr>
            <tr><td colspan="3" style="text-align:right"><strong>Total:</strong></td><td><strong>@Model.Total</strong></td></tr>
        </tfoot>
    </table>

    <p><a href="@Url.Action("Orders","Shop")" class="back-link">Back to orders</a></p>
</div>

<style>

    body {
        background-color: #ffffff;
        font-family: 'Comic Sans MS', cursive, sans-serif;
        margin: 0;
        padding: 0;
        min-height: 100vh;
    }
    .order-details {
        max-width: 1000px;
        margin: 40px auto;
        background: #ffffff;
        border: 2px solid #e5c185;
        border-radius: 20px;
        padding: 40px;
        box-shadow: 0 8px 25px rgba(0,0,0,0.1);
        overflow: hidden;
        padding: 24px;
    }
        /* rounded corners and soft shadow for a card feel */ /* [13][12] */

        /* header */
        .order-details h2 {
            color: #6b4b2e;
            margin: 0 0 8px 0;
            font-weight: 700;
        }

    .order-meta {
        color: #6b4b2e;
        margin: 0 0 18px 0;
    }

    /* table */
    .order-table {
        width: 100%;
        border-collapse: separate;
        border-spacing: 0;
        font-size: 14px;
    }
        /* use standard table styling patterns */ /* [1][2] */

        .order-table thead th {
            background: #fff1c1;
            color: #6b4b2e;
            text-align: left;
            font-weight: 600;
            padding: 12px 14px;
            border-bottom: 2px solid #f0e6d2;
        }
        /* clear header banding and separation */ /* [1][4] */

        .order-table tbody td {
            color: #6b4b2e;
            padding: 12px 14px;
            border-bottom: 1px solid #f0e6d2;
        }
        /* row delineation for readability */ /* [1] */

        .order-table tbody tr:nth-of-type(even) {
            background: #fff9eb;
        }
        /* zebra rows */ /* [1] */
        .order-table tbody tr:hover {
            background: #fff1c1;
            transition: background-color 0.15s ease-in-out;
        }
        /* hover feedback */ /* [20] */

        /* align numbers right, text left */
        .order-table td:nth-child(2),
        .order-table td:nth-child(3),
        .order-table td:nth-child(4),
        .order-table tfoot td:last-child {
            text-align: right;
        }
        /* numeric columns right-aligned */ /* [1] */

        .order-table tfoot td {
            padding: 10px 14px;
            color: #6b4b2e;
            font-weight: 600;
            border-top: 2px solid #f0e6d2;
        }

        .order-table thead th:first-child {
            border-top-left-radius: 12px;
        }

        .order-table thead th:last-child {
            border-top-right-radius: 12px;
        }

    .back-link {
        display: inline-block;
        margin-top: 16px;
        text-decoration: none;
        background-color: transparent;
        border: 1px solid #e5c185;
        padding: 12px 20px;
        border-radius: 30px;
        color: #6b4b2e;
        font-weight: bold;
        transition: all 0.3s ease;
    }

        .back-link:hover {
            background-color: #fff1c1;
            transform: translateY(-2px);
        }

    @@media (max-width: 768px) {
        .order-details

    {
        padding: 16px;
    }

    .order-table th, .order-table td {
        padding: 10px 12px;
    }
    }
</style>