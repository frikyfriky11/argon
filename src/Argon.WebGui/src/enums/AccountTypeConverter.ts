import { AccountType } from "../services/backend/BackendClient";

function convert(value: AccountType | undefined): string {
  switch (value) {
    case AccountType.Setup:
      return "Conto iniziale";
    case AccountType.Cash:
      return "Conto liquidità";
    case AccountType.Expense:
      return "Conto di spesa";
    case AccountType.Revenue:
      return "Conto di ricavo";
    case AccountType.Debit:
      return "Conto di debito";
    case AccountType.Credit:
      return "Conto di credito";
    case undefined:
      return "";
  }
}

export default {
  convert,
};
