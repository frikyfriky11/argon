import { LocalizationProvider } from "@mui/x-date-pickers";
import { AdapterLuxon } from "@mui/x-date-pickers/AdapterLuxon";
import { lazy } from "react";
import { useTranslation } from "react-i18next";
import { BrowserRouter, Route, Routes } from "react-router-dom";

import ProtectedRoute from "./ProtectedRoute.tsx";

const AuthLayout = lazy(() => import("../pages/Auth/Layout"));
const AuthPostSignIn = lazy(() => import("../pages/Auth/PostSignIn"));
const Layout = lazy(() => import("../components/Layout"));
const HomePage = lazy(() => import("../pages/HomePage"));
const Dashboard = lazy(() => import("../pages/Dashboard"));
const AccountsList = lazy(() => import("../pages/Accounts/List"));
const AccountsAdd = lazy(() => import("../pages/Accounts/Add"));
const AccountsEdit = lazy(() => import("../pages/Accounts/Edit"));
const BudgetList = lazy(() => import("../pages/Budget/List"));
const TransactionsList = lazy(() => import("../pages/Transactions/List"));
const TransactionsAdd = lazy(() => import("../pages/Transactions/Add"));
const TransactionsEdit = lazy(() => import("../pages/Transactions/Edit"));
const SystemLayout = lazy(() => import("../pages/System/Layout"));
const SystemNotFound = lazy(() => import("../pages/System/NotFound"));

export default function MainRouter() {
  const { i18n } = useTranslation();

  return (
    /* we need to inject the LocalizationProvider here because
     * the locale is set by i18next and we need to know which
     * locale was chosen by the user using its dedicated hook */
    <LocalizationProvider
      adapterLocale={i18n.language}
      dateAdapter={AdapterLuxon}
    >
      <BrowserRouter>
        <Routes>
          <Route element={<AuthLayout />} path="/auth">
            <Route element={<AuthPostSignIn />} path="/auth/post-sign-in" />
          </Route>
          <Route element={<ProtectedRoute />}>
            <Route element={<Layout />} path="/">
              <Route element={<HomePage />} index />
              <Route element={<Dashboard />} path="/dashboard" />
              <Route element={<AccountsList />} path="/accounts" />
              <Route element={<AccountsAdd />} path="/accounts/add" />
              <Route element={<AccountsEdit />} path="/accounts/:id" />
              <Route element={<BudgetList />} path="/budget" />
              <Route element={<TransactionsList />} path="/transactions" />
              <Route element={<TransactionsAdd />} path="/transactions/add" />
              <Route element={<TransactionsEdit />} path="/transactions/:id" />
            </Route>
          </Route>
          <Route element={<SystemLayout />}>
            <Route element={<SystemNotFound />} path="*" />
          </Route>
        </Routes>
      </BrowserRouter>
    </LocalizationProvider>
  );
}
