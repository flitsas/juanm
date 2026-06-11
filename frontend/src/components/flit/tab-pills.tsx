type TabPillsProps<T extends string> = {
  tabs: { id: T; label: string }[];
  active: T;
  onChange: (id: T) => void;
  ariaLabel: string;
};

export function TabPills<T extends string>({
  tabs,
  active,
  onChange,
  ariaLabel,
}: TabPillsProps<T>) {
  return (
    <nav aria-label={ariaLabel} className="mb-4 flex flex-wrap gap-2">
      {tabs.map((tab) => {
        const isActive = tab.id === active;
        return (
          <button
            key={tab.id}
            type="button"
            onClick={() => onChange(tab.id)}
            aria-current={isActive ? "page" : undefined}
            className={
              isActive
                ? "flit-tab-pill flit-tab-pill-active"
                : "flit-tab-pill flit-tab-pill-inactive"
            }
          >
            {tab.label}
          </button>
        );
      })}
    </nav>
  );
}
