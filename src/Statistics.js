export default function Statistics({ score, elements_count }) {
  return (
    <div className="stats-container">
      <div className="score-container">
        Score {score}
      </div>
      {Object.keys(elements_count).map((element, i) => {
        return (
          <div className="elements-count-container" key={element}>
            {element}:{elements_count[element]}
          </div>
        );
      }
      )}
    </div>
  );
}