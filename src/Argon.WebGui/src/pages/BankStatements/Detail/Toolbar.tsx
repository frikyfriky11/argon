import { Box, BoxProps, Typography } from "@mui/material";

export type ToolbarProps = {
  fileName: string;
} & BoxProps;

export default function Toolbar({ fileName, ...props }: ToolbarProps) {
  return (
    <Box {...props}>
      <Box
        justifyContent="space-between"
        sx={{
          alignItems: "center",
          display: "flex",
          flexWrap: "wrap",
        }}
      >
        <Typography variant="h4">{fileName}</Typography>
      </Box>
    </Box>
  );
}
