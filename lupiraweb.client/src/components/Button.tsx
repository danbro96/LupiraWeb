export interface ButtonProps {
  children: React.ReactNode;
  onClick?: () => void;
  className?: string;
  type?: "button" | "submit" | "reset";
}

export default function Button({
  children,
  onClick,
  className = "",
  type = "button",
  ...props
}: ButtonProps): React.ReactElement {
  return (
    <button
      type={type}
      onClick={onClick}
      className={`bg-teal-600 hover:bg-teal-700 text-white px-4 py-2 rounded ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}
