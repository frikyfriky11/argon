import TranslateIcon from "@mui/icons-material/Translate";
import {
  Checkbox,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Popover,
} from "@mui/material";
import { DE, GB, IT, US } from "country-flag-icons/react/3x2";
import React, { useState } from "react";
import { useTranslation } from "react-i18next";

export default function LanguageSelector() {
  const { i18n } = useTranslation();

  const [anchorEl, setAnchorEl] = useState<HTMLButtonElement | null>(null);

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const open = Boolean(anchorEl);

  const languages = [
    {
      code: "it-IT",
      name: "Italiano",
      icon: <IT height={12} />,
    },
    {
      code: "en-US",
      name: "English (US)",
      icon: <US height={12} />,
    },
    {
      code: "en-GB",
      name: "English (UK)",
      icon: <GB height={12} />,
    },
    {
      code: "de-DE",
      name: "Deutsch",
      icon: <DE height={12} />,
    },
  ];

  const changeLanguage = async (code: string) => {
    await i18n.changeLanguage(code);
  };

  return (
    <div>
      <IconButton onClick={handleClick}>
        <TranslateIcon />
      </IconButton>
      <Popover
        anchorEl={anchorEl}
        anchorOrigin={{
          vertical: "bottom",
          horizontal: "right",
        }}
        onClose={handleClose}
        open={open}
      >
        <List disablePadding>
          {languages.map((language, index) => (
            <ListItem
              disablePadding
              key={index}
              secondaryAction={
                <Checkbox
                  checked={i18n.language === language.code}
                  disableRipple
                  edge="end"
                  onClick={() => void changeLanguage(language.code)}
                />
              }
            >
              <ListItemButton
                onClick={() => void changeLanguage(language.code)}
              >
                <ListItemIcon>{language.icon}</ListItemIcon>
                <ListItemText primary={language.name} />
              </ListItemButton>
            </ListItem>
          ))}
        </List>
      </Popover>
    </div>
  );
}
