import ArrowForwardIcon from "@mui/icons-material/ArrowForward";
import {
  Button,
  Card,
  CardActions,
  CardContent,
  Grid,
  Stack,
  Typography,
} from "@mui/material";
import React from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";

import { IAccountsGetListResponse } from "../../services/backend/BackendClient";

export type FavouriteAccountsProps = {
  accounts: IAccountsGetListResponse[];
};

export default function FavouriteAccounts({
  accounts,
}: FavouriteAccountsProps) {
  const { i18n } = useTranslation();

  const favouriteAccounts = accounts.filter((account) => account.isFavourite);

  return (
    <Stack gap={2}>
      <Typography variant="h5">Conti preferiti</Typography>
      {favouriteAccounts.length === 0 ? (
        <>
          <Typography>
            Imposta uno o più conti come preferiti e verranno mostrati qui.
          </Typography>

          <Stack direction="row">
            <Button
              color="primary"
              component={Link}
              endIcon={<ArrowForwardIcon />}
              size="small"
              to="/accounts"
              variant="text"
            >
              Vai ai conti
            </Button>
          </Stack>
        </>
      ) : (
        <Grid container spacing={2}>
          {favouriteAccounts.map((account) => (
            <Grid item key={account.id} lg={3} md={4} sm={6} xs={12}>
              <Card>
                <CardContent>
                  <Typography
                    display="inline-block"
                    overflow="hidden"
                    textOverflow="ellipsis"
                    title={account.name}
                    variant="overline"
                    whiteSpace="nowrap"
                    width="100%"
                  >
                    {account.name}
                  </Typography>
                  <Typography variant="h4">
                    {account.totalAmount.toLocaleString(i18n.language, {
                      style: "currency",
                      currency: "EUR",
                    })}
                  </Typography>
                </CardContent>
                <CardActions>
                  <Button
                    color="primary"
                    component={Link}
                    endIcon={<ArrowForwardIcon />}
                    size="small"
                    to={`/accounts/${account.id}`}
                    variant="text"
                  >
                    Vedi
                  </Button>
                </CardActions>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}
    </Stack>
  );
}
