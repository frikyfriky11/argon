import WarningIcon from "@mui/icons-material/Warning";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
} from "@mui/material";

export type ParsingWarningsDialogProps = {
  open: boolean;
  onClose: () => void;
  warnings: string[];
};

export default function ParsingWarningsDialog({
  open,
  onClose,
  warnings,
}: ParsingWarningsDialogProps) {
  return (
    <Dialog onClose={onClose} open={open} maxWidth="md" fullWidth>
      <DialogTitle variant="h6">
        Avvisi di Parsing ({warnings.length})
      </DialogTitle>
      <DialogContent dividers>
        <List>
          {warnings.map((warning, index) => (
            <ListItem key={index} disablePadding>
              <ListItemIcon>
                <WarningIcon color="warning" />
              </ListItemIcon>
              <ListItemText primary={warning} />
            </ListItem>
          ))}
        </List>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="primary" variant="contained">
          Chiudi
        </Button>
      </DialogActions>
    </Dialog>
  );
}
