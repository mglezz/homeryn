# Homeryn

### Your home, in sync.

Homeryn is a project to make sharing a home a little easier. The aim is to bring shopping lists, household chores, and shared responsibilities into one place, so there is less to remember and less to chase up.

From adding milk to the shopping list to checking whose turn it is to clean the kitchen, everyday coordination should be simple.

## What's planned?

- Share shopping lists with the people at home.
- Add products and mark them as bought.
- Create chores, assign them, and mark them as done.
- Keep everyone in a household up to date.
- Save household information for the next time the app is opened.

Homeryn is intended for families, couples, and roommates. Shopping lists are taking shape; chores and household accounts are still planned.

## Project status

**The first shared shopping list is working. Homeryn is still an early local prototype.**

The shopping window lets you add items, change quantities, mark purchases, and remove items. Your list stays saved when the app and server close. Several copies of the desktop app can use the same local server.

Use **Start server**, then **Shopping list**. Enter an item and quantity and click **Add**. Select a row to change it and click **Save**, or use **Remove**. Click **Refresh** to see changes from another window. If someone has already changed the same item, the app asks you to refresh before trying again.

The solution must be built first; setup instructions are in the [development guide](docs/development.md). **Check connection** is still available to check the server. There is one list per server, all on one computer. Chores, accounts, invitations, separate households, and access from other devices are not available yet.

## Why Homeryn?

I started Homeryn as my final project for Distributed Systems and Parallel Programming. I also want to keep developing it into a useful application and a project for my portfolio.

The approach is to start small, understand each decision, and add features step by step.

## Follow the progress

Design notes, setup instructions, and the development record are available in the [engineering documentation](docs/README.md).

Recurring chores, reminders, and access from more devices are possible future additions. There are no commitments to those features or a release date.

**Homeryn — Your home, in sync.**
