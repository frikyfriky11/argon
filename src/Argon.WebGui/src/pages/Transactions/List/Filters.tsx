import React from "react";

import AccountsFilter from "./AccountsFilter";

export type FiltersProps = {
  onAccountIdsChange: (value: string[]) => void;
  accountIds: string[];
};

export default function Filters({
  onAccountIdsChange,
  accountIds,
}: FiltersProps) {
  return (
    <>
      <AccountsFilter onChange={onAccountIdsChange} values={accountIds} />
    </>
  );
}
