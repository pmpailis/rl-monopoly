import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;
import com.google.gson.stream.JsonReader;
import org.dinbur.ml.rl.monopoly.game.properties.MonopolyProperty;

import java.io.FileNotFoundException;
import java.io.FileReader;
import java.lang.reflect.Type;
import java.util.List;

import static org.dinbur.ml.rl.monopoly.game.properties.MonopolyProperty.createGson;

public class Main {

    private static final Type REVIEW_TYPE = new TypeToken<List<MonopolyProperty>>() {
    }.getType();

    public static void main(String[] args) throws FileNotFoundException {
        Gson gson = createGson();
        JsonReader reader = new JsonReader(new FileReader("/home/alduin/workspace/github/rl-monopoly/game_resources/game_properties.json"));
        List<MonopolyProperty> data = gson.fromJson(reader, REVIEW_TYPE); // contains the whole reviews list
        System.out.println(data);
    }
}
